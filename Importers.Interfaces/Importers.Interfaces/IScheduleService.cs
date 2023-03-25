using TimetablePlanning.Importers.Model;

namespace TimetablePlanning.Importers.Interfaces;

public interface IScheduleService : IScheduleSource, IScheduleDestination { }

public interface IScheduleSource
{
    ImportResult<Schedule> GetSchedule(string filename);
}

public interface IScheduleDestination
{
    ImportResult<Schedule> Save(Schedule filename);
}
