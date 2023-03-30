using System.Data;

namespace TimetablePlanning.Importers.Xpln.DataSetProviders;
public interface IDataSetProvider
{
    DataSet? ImportSchedule(Stream stream, DataSetConfiguration configiration);
}
