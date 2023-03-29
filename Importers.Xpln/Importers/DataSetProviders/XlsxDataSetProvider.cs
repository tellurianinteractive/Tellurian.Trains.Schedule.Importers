using ExcelDataReader;
using Microsoft.Extensions.Logging;
using System.Data;
using TimetablePlanning.Importers.Xpln.Extensions;

namespace TimetablePlanning.Importers.Xpln.DataSetProviders;

public sealed class XlsxDataSetProvider : IDataSetProvider
{
    private readonly ILogger Logger;
    public XlsxDataSetProvider( ILogger logger)
    {
        Logger = logger;
    }
    public string[] GetRowData(DataRow row) => row.GetRowFields();
    public DataSet? LoadFromFile(Stream stream, DataSetConfiguration configuration)
    {
        var worksheets = configuration.Worksheets;
        try
        {
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
            Logger.LogError(ex, "Error when reading {file}.", configuration.Name);
            throw;
        }
    }

    
}
