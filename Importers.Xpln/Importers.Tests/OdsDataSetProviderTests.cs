using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using TimetablePlanning.Importers.Xpln.DataSetProviders;

namespace TimetablePlanning.Importers.Xpln.Tests;

[TestClass]
public class OdsDataSetProviderTests
{
    [TestMethod]
    public void ReadsFile()
    {
        var target = new OdsDataSetProvider(new DirectoryInfo("Test data"), NullLogger.Instance);
        var dataSet = target.LoadFromFile("Montan2023H0e.ods", DataSetConfiguration());
        Assert.IsNotNull(dataSet);
        WriteDataSet(dataSet, "Test data\\Montan2023H0e");
    }

    private static DataSetConfiguration DataSetConfiguration()
    {
        var result = new DataSetConfiguration();
        result.Add(new WorksheetConfiguration("StationTrack", 8));
        result.Add(new WorksheetConfiguration("Routes", 11));
        result.Add(new WorksheetConfiguration("Trains", 11));
        return result;
    }

    private static void WriteDataSet(DataSet dataSet, string fileName)
    {
        foreach (DataTable table in dataSet.Tables)
        {
            using var file = File.OpenWrite($"{fileName}-{table.TableName}.txt");
            var writer = new StreamWriter(file);
            foreach (DataRow row in table.Rows)
            {
                foreach (var cell in row.ItemArray)
                {
                    writer.Write(cell);
                    writer.Write(";");
                }
                writer.WriteLine();
            }
            writer.Flush();
            file.Close();
        }
    }
}
