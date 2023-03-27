using Microsoft.Extensions.Logging;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using System.Xml;
using TimetablePlanning.Importers.Xpln.Extensions;

namespace TimetablePlanning.Importers.Xpln.DataSetProviders;

public sealed class OdsDataSetProvider : IDataSetProvider
{
    private const string DefaultDocumentSuffix = ".ods";
    private readonly ILogger Logger;
    private readonly DirectoryInfo DocumentsDirectory; 
    public OdsDataSetProvider(DirectoryInfo documentsDirectory, ILogger logger)
    {
        Logger = logger;
        DocumentsDirectory = documentsDirectory ?? throw new ArgumentNullException(nameof(documentsDirectory));
        if (!DocumentsDirectory.Exists) throw new DirectoryNotFoundException(DocumentsDirectory.FullName);
    }
    public string[] GetRowData(DataRow row) => row.GetRowFields();

    private string GetFullFilename(string fileName)
    {
        if (fileName.HasFileExtension(DefaultDocumentSuffix) && File.Exists(fileName)) return fileName;
        return Path.Combine(DocumentsDirectory.FullName, string.IsNullOrEmpty(Path.GetExtension(fileName)) ? fileName + DefaultDocumentSuffix : fileName);
    }

    public DataSet? LoadFromFile(string filename, params string[]? worksheets)
    {
        worksheets ??= Array.Empty<string>();
        try
        {
            using var stream = GetStream(GetFullFilename(filename));
            using var archive = GetZipArchive(stream);
            var document = GetContentXmlFile(archive);
            var namespaceMananger = InitializeXmlNamespaceManager(document);
            var dataSet = new DataSet(Path.GetFileName(filename));
            var tables = GetDataTables(document, worksheets, namespaceMananger);
            dataSet.Tables.AddRange(tables.ToArray());
            return dataSet;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error when reading {file}.", filename);
            throw;
        }
    }

    private static IEnumerable<DataTable> GetDataTables(XmlDocument document, string[] worksheets, XmlNamespaceManager namespaceManager)
    {
        var tableNodes = TableNodes(document, namespaceManager);
        if (tableNodes is not null)
        {
            foreach (XmlNode tableNode in tableNodes)
            {
                var table = GetSheet(tableNode, worksheets, namespaceManager);
                if (table is null) continue;
                yield return table;
            }
        }
    }

    private static XmlNodeList? TableNodes(XmlDocument document, XmlNamespaceManager namespaceManager)
    {
        return document.SelectNodes("/office:document-content/office:body/office:spreadsheet/table:table", namespaceManager);
    }

    private static DataTable? GetSheet(XmlNode tableNode, string[] worksheets, XmlNamespaceManager namespaceManager)
    {

        var nameAttribute = tableNode.Attributes?["table:name"];
        if (nameAttribute is null) return null;
        if (IsSheetIncluded(nameAttribute, worksheets))
        {
            DataTable sheet = new DataTable(nameAttribute.Value);
            var rowNodes = tableNode.SelectNodes("table:table-row", namespaceManager);
            if (rowNodes is not null) {
                int rowIndex = 0;
                foreach (XmlNode rowNode in rowNodes)
                {
                    GetRow(rowNode, sheet, namespaceManager, ref rowIndex);
                }
            }
            return sheet;

        }
        return null;

        static bool IsSheetIncluded(XmlAttribute nameAttribute, string[] worksheets) =>
            worksheets.Length == 0 || worksheets.Contains(nameAttribute.Value, StringComparer.OrdinalIgnoreCase);
    }

    private static void GetRow(XmlNode rowNode, DataTable sheet, XmlNamespaceManager namespaceManager, ref int rowIndex)
    {
        var rowsRepeated = rowNode.Attributes?["table:number-rows-repeated"];
        if (rowsRepeated is null || Convert.ToInt32(rowsRepeated.Value, CultureInfo.InvariantCulture) == 1)
        {
            while (sheet.Rows.Count < rowIndex) sheet.Rows.Add(sheet.NewRow());
            var row = sheet.NewRow();
            var cellNodes = rowNode.SelectNodes("table:table-cell", namespaceManager);
            int cellIndex = 0;
            foreach (XmlNode cellNode in cellNodes!)
                GetCell(cellNode, row, ref cellIndex);
            sheet.Rows.Add(row);
            rowIndex++;
        }
        else
        {
            rowIndex += Convert.ToInt32(rowsRepeated.Value, CultureInfo.InvariantCulture);
        }
        if (sheet.Rows.Count == 0)
        {
            sheet.Rows.Add(sheet.NewRow());
            sheet.Columns.Add();
        }
    }

    private static void GetCell(XmlNode cellNode, DataRow row, ref int cellIndex)
    {
        var cellRepeated = cellNode.Attributes?["table:number-columns-repeated"];
            DataTable sheet = row.Table;
        if (cellRepeated == null)
        {
            if (sheet.Columns.Count <= cellIndex)
                sheet.Columns.Add();
            row[cellIndex] = ReadCellValue(cellNode);
            cellIndex++;
        }
        else
        {
            var repeated = Convert.ToInt32(cellRepeated.Value, CultureInfo.InvariantCulture);
            if (cellIndex + repeated < sheet.Columns.Count)
            {
                for(int i = 0; i < repeated; i++)
                {
                    if (sheet.Columns.Count <= cellIndex)
                        sheet.Columns.Add();
                    row[cellIndex] = ReadCellValue(cellNode);
                    cellIndex++;
                }

            }
        }
    }
    private static string? ReadCellValue(XmlNode cell)
    {
        var cellVal = cell.Attributes?["office:value"];
        if (cellVal is null)
            return string.IsNullOrEmpty(cell.InnerText) ? null : cell.InnerText;
        else
            return cellVal.Value;
    }

    private static XmlDocument GetContentXmlFile(ZipArchive archive)
    {
        const string entryName = "content.xml";
        var entry = archive.GetEntry(entryName) ?? throw new FileNotFoundException(entryName);
        using var stream = entry.Open();
        XmlDocument document = new XmlDocument();
        document.Load(stream);
        return document;
    }

    private static XmlNamespaceManager InitializeXmlNamespaceManager(XmlDocument xmlDocument)
    {
        var manager = new XmlNamespaceManager(xmlDocument.NameTable);
        foreach (var ns in Namespaces)
        {
            manager.AddNamespace(ns.Key, ns.Value);
        };
        return manager;
    }

    private static Stream GetStream(string filename)
    {
        if (File.Exists(filename))
        {
            return new FileStream(filename, FileMode.Open, FileAccess.Read);
        }
        throw new FileNotFoundException(filename);
    }

    private static ZipArchive GetZipArchive(Stream stream)
    {
        return new ZipArchive(stream);
    }

    private static readonly Dictionary<string, string> Namespaces = new()
    {
        { "table", "urn:oasis:names:tc:opendocument:xmlns:table:1.0" },
        { "office", "urn:oasis:names:tc:opendocument:xmlns:office:1.0" },
        { "style", "urn:oasis:names:tc:opendocument:xmlns:style:1.0" },
        { "text", "urn:oasis:names:tc:opendocument:xmlns:text:1.0" },
        { "draw", "urn:oasis:names:tc:opendocument:xmlns:drawing:1.0" },
        { "fo", "urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0" },
        { "dc", "http://purl.org/dc/elements/1.1/" },
        { "meta", "urn:oasis:names:tc:opendocument:xmlns:meta:1.0" },
        { "number", "urn:oasis:names:tc:opendocument:xmlns:datastyle:1.0" },
        { "presentation", "urn:oasis:names:tc:opendocument:xmlns:presentation:1.0" },
        { "svg", "urn:oasis:names:tc:opendocument:xmlns:svg-compatible:1.0" },
        { "chart", "urn:oasis:names:tc:opendocument:xmlns:chart:1.0" },
        { "dr3d", "urn:oasis:names:tc:opendocument:xmlns:dr3d:1.0" },
        { "math", "http://www.w3.org/1998/Math/MathML" },
        { "form", "urn:oasis:names:tc:opendocument:xmlns:form:1.0" },
        { "script", "urn:oasis:names:tc:opendocument:xmlns:script:1.0" },
        { "ooo", "http://openoffice.org/2004/office" },
        { "ooow", "http://openoffice.org/2004/writer" },
        { "oooc", "http://openoffice.org/2004/calc" },
        { "dom", "http://www.w3.org/2001/xml-events" },
        { "xforms", "http://www.w3.org/2002/xforms" },
        { "xsd", "http://www.w3.org/2001/XMLSchema" },
        { "xsi", "http://www.w3.org/2001/XMLSchema-instance" },
        { "rpt", "http://openoffice.org/2005/report" },
        { "of", "urn:oasis:names:tc:opendocument:xmlns:of:1.2" },
        { "rdfa", "http://docs.oasis-open.org/opendocument/meta/rdfa#" },
        { "config", "urn:oasis:names:tc:opendocument:xmlns:config:1.0" }
    };

}