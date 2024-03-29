﻿using ExcelDataReader;
using Microsoft.Extensions.Logging;
using System.Data;
using TimetablePlanning.Importers.Xpln.Extensions;

namespace TimetablePlanning.Importers.Xpln.DataSetProviders;

public sealed class XlsxDataSetProvider(ILogger logger) : IDataSetProvider
{
    private readonly ILogger Logger = logger;

    public static string[] GetRowData(DataRow row) => row.GetRowFields();
    public DataSet? ImportSchedule(Stream stream, DataSetConfiguration configuration)
    {
        var worksheets = configuration.Worksheets;
        try
        {
            using var reader = ExcelReaderFactory.CreateReader(stream);
            var dataSet = reader.AsDataSet();
            if (worksheets.Length > 0)
            {
                foreach (DataTable table in dataSet.Tables)
                {
                    if (!worksheets.Any(w => w.Equals(table.TableName, StringComparison.OrdinalIgnoreCase)))
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
