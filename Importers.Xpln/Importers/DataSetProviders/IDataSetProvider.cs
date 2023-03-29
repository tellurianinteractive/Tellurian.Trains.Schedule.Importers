using System.Data;

namespace TimetablePlanning.Importers.Xpln.DataSetProviders;
public interface IDataSetProvider
{
    DataSet? LoadFromFile(string filename, DataSetConfiguration configiration);
    string[] GetRowData(DataRow dataTable);
}
