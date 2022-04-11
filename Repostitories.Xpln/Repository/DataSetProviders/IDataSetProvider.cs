using System.Data;

namespace Tellurian.Trains.Repositories.Xpln.DataSetProviders;
public interface IDataSetProvider
{
    DataSet? LoadFromFile(string filename, params string[]? worksheets);
    string[] GetRowData(DataRow dataTable);
}
