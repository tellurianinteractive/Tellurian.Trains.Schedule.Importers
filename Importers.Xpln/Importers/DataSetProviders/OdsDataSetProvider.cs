using Microsoft.Extensions.Logging;
using System.Data;
using System.Globalization;
using System.IO.Compression;
using System.Xml;
using TimetablePlanning.Importers.Model;
using TimetablePlanning.Importers.Xpln.Extensions;

namespace TimetablePlanning.Importers.Xpln.DataSetProviders;

public sealed class OdsDataSetProvider : IDataSetProvider
{
    private readonly ILogger Logger;

    public OdsDataSetProvider(ILogger<OdsDataSetProvider> logger)
    {
        Logger = logger;
    }

    public DataSet? ImportSchedule(Stream inputStream, DataSetConfiguration dataSetConfiguration)
    {
        try
        {
            using var archive = GetZipArchive(inputStream);
            var document = GetContentXmlFile(archive);
            var namespaceMananger = InitializeXmlNamespaceManager(document);
            var dataSet = new DataSet(dataSetConfiguration.Name);
            var tables = GetDataTables(document, dataSetConfiguration, namespaceMananger);
            dataSet.Tables.AddRange(tables.ToArray());
            return dataSet;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error when reading {stream}.", inputStream.ToString());
            throw;
        }
    }

    private IEnumerable<DataTable> GetDataTables(XmlDocument document, DataSetConfiguration configuration, XmlNamespaceManager namespaceManager)
    {
        var tableNodes = TableNodes(document, namespaceManager);
        Logger.LogInformation("{count} table nodes in document.", tableNodes?.Count ?? 0);
        if (tableNodes is not null)
        {
            foreach (XmlNode tableNode in tableNodes)
            {
                var nameAttribute = tableNode.Attributes?["table:name"];
                var worksheetConfiguration = configuration.WorksheetConfiguration(nameAttribute?.Value);
                if (worksheetConfiguration is not null)
                {
                    Logger.LogInformation("Reading table {table}.", worksheetConfiguration.WorksheetName);
                    var table = GetDataTable(tableNode, worksheetConfiguration, namespaceManager);
                    if (table is null) continue;
                    yield return table;
                }
            }
        }
    }

    private static XmlNodeList? TableNodes(XmlDocument document, XmlNamespaceManager namespaceManager)
    {
        return document.SelectNodes("/office:document-content/office:body/office:spreadsheet/table:table", namespaceManager);
    }

    private static DataTable? GetDataTable(XmlNode tableNode, WorksheetConfiguration configuration, XmlNamespaceManager namespaceManager)
    {
        var nameAttribute = tableNode.Attributes?["table:name"];
        if (nameAttribute is null) return null;

        DataTable dataTable = new DataTable(nameAttribute.Value);
        var rowNodes = tableNode.SelectNodes("table:table-row", namespaceManager);
        if (rowNodes is not null)
        {
            int rowIndex = 0;
            foreach (XmlNode rowNode in rowNodes)
            {
                var isRead = GetRow(rowNode, dataTable, namespaceManager, configuration, ref rowIndex);
                if (!isRead) break; ;
            }
        }
        if (dataTable.Rows.Count == 0)
        {
            dataTable.Rows.Add(dataTable.NewRow());
            dataTable.Columns.Add();
        }
        return dataTable;
    }

    private static bool GetRow(XmlNode rowNode, DataTable dataTable, XmlNamespaceManager namespaceManager, WorksheetConfiguration configuration, ref int rowIndex)
    {
        var rowsRepeated = rowNode.Attributes?["table:number-rows-repeated"];
        var repeat = rowsRepeated is null ? 1 : Convert.ToInt32(rowsRepeated.Value, CultureInfo.InvariantCulture);
        if (repeat > configuration.MaxRowRepetitions) return false;
        for (var i = 0; i < repeat; i++)
        {
            var row = dataTable.NewRow();
            while (dataTable.Columns.Count < configuration.MaxReadColumns)
                dataTable.Columns.Add();

            var cellNodes = rowNode.SelectNodes("table:table-cell", namespaceManager);
            int cellIndex = 0;
            foreach (XmlNode cellNode in cellNodes!)
            {
                GetCell(cellNode, row, configuration.MaxReadColumns, ref cellIndex);
                if (cellIndex >= configuration.MaxReadColumns) break;
            }
            if (HasValue(row)) dataTable.Rows.Add(row);
            rowIndex++;
        }
        return true;

        static bool HasValue(DataRow row) => row.GetRowFields().Any(f => f.HasText());
    }

    private static void GetCell(XmlNode cellNode, DataRow row, int columns, ref int cellIndex)
    {
        var cellRepeated = cellNode.Attributes?["table:number-columns-repeated"];
        var repeat = cellRepeated is null ? 1 : Convert.ToInt32(cellRepeated.Value, CultureInfo.InvariantCulture);
        for (int i = 0; i < repeat; i++)
        {
            if (cellIndex >= columns) break;
            row[cellIndex] = ReadCellValue(cellNode);
            cellIndex++;
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