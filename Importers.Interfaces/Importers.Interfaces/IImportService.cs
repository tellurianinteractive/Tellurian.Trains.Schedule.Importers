using TimetablePlanning.Importers.Model;

namespace TimetablePlanning.Importers.Interfaces;

public interface IImportService
{
    ImportResult<Schedule> ImportSchedule(string name);

}
