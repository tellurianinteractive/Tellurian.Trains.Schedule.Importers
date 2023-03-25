using ExcelDataReader;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.IO;
using System.Linq;
using Tellurian.Trains.Repositories.Xpln.Extensions;

namespace Tellurian.Trains.Repositories.Xpln.DataSetProviders;

public sealed class XlsxDataSetProvider : IDataSetProvider
{
    private const string DefaultDocumentSuffix = ".xlsx";
    private readonly ILogger Logger;
    private readonly DirectoryInfo DocumentsDirectory;
    public XlsxDataSetProvider(DirectoryInfo documentsDirectory, ILogger logger)
    {
        Logger = logger;
        DocumentsDirectory = documentsDirectory ?? throw new ArgumentNullException(nameof(documentsDirectory));
        if (!DocumentsDirectory.Exists) throw new DirectoryNotFoundException(DocumentsDirectory.FullName);
    }
    public string[] GetRowData(DataRow row) => row.GetRowFields();
    public DataSet? LoadFromFile(string filename, params string[]? worksheets)
    {
        if (worksheets is null) worksheets = Array.Empty<string>();
        try
        {
            var excelDocumentFilename = GetFullFilename(filename);
            using var stream = File.Open(excelDocumentFilename, FileMode.Open, FileAccess.Read);
            using var reader = ExcelReaderFactory.CreateReader(stream);
            var dataSet = reader.AsDataSet();
            if (worksheets.Any())
            {
                foreach(DataTable table in dataSet.Tables)
                {
                    if (! worksheets.Any(w  => w.Equals(table.TableName, StringComparison.OrdinalIgnoreCase)))
                    {
                        dataSet.Tables.Remove(table);
                    }
                }
            }
            return dataSet;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error when reading {file}.", filename);
            throw;
        }
    }

    private string GetFullFilename(string fileName)
    {
        if (fileName.HasFileExtension(DefaultDocumentSuffix, ".xls") && File.Exists(fileName)) return fileName;
        return Path.Combine(DocumentsDirectory.FullName, string.IsNullOrEmpty(Path.GetExtension(fileName)) ? fileName + DefaultDocumentSuffix : fileName);
    }
}
