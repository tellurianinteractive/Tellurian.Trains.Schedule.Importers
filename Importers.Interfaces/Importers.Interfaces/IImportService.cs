using TimetablePlanning.Importers.Model;

namespace TimetablePlanning.Importers.Interfaces;

public interface IImportService
{
    ImportResult<Schedule> ImportSchedule(FileInfo inputFile, string name);
    ImportResult<Schedule> ImportSchedule(Stream inputStream, string name);

}
