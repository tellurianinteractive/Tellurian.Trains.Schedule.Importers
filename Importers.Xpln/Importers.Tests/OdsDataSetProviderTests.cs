using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using System.IO.MemoryMappedFiles;
using TimetablePlanning.Importers.Xpln.DataSetProviders;

namespace TimetablePlanning.Importers.Xpln.Tests;

[TestClass]
public class OdsDataSetProviderTests
{
    [TestMethod]
    public void ReadsFile()
    {
        const string path = "Test data\\Montan2023H0e.ods";
        var target = new OdsDataSetProvider(NullLogger<OdsDataSetProvider>.Instance);
        using var stream = File.OpenRead(path);
        var dataSet = target.ImportSchedule(stream, DataSetConfiguration());
        Assert.IsNotNull(dataSet);
        WriteDataSet(dataSet, path);
    }

    private static DataSetConfiguration DataSetConfiguration()
    {
        var result = new DataSetConfiguration("XplnDocument");
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
