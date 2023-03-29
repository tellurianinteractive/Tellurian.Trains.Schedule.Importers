using System.Data;

namespace TimetablePlanning.Importers.Xpln.DataSetProviders;
public interface IDataSetProvider
{
    DataSet? LoadFromFile(Stream stream, DataSetConfiguration configiration);
    string[] GetRowData(DataRow dataTable);
}
