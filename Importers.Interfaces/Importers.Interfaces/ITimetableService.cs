using TimetablePlanning.Importers.Model;

namespace TimetablePlanning.Importers.Interfaces;

public interface ITimetableService: IScheduleSourceService, IScheduleDestinationService { }

public interface IScheduleSourceService
{
    ImportResult<Schedule> GetSchedule(FileInfo inputFile, string name);
    ImportResult<Schedule> GetSchedule(Stream inputStream, string name);

}

public interface IScheduleDestinationService
{
    ImportResult<Schedule> Save(Timetable timetable);
}
