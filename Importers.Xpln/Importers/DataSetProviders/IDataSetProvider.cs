using System.Data;

namespace TimetablePlanning.Importers.Xpln.DataSetProviders;
public interface IDataSetProvider
{
    DataSet? LoadFromFile(string filename, params string[]? worksheets);
    string[] GetRowData(DataRow dataTable);
}
