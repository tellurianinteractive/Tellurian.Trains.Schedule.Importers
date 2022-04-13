using Tellurian.Trains.Models.Planning;

namespace Tellurian.Trains.Repositories.Interfaces
{
    public interface ITimetableStore: ITimetableReadStore, ITimetableWriteStore { }

    public interface ITimetableReadStore
    {
        RepositoryResult<Timetable> GetTimetable(string name);
    }

    public interface ITimetableWriteStore
    {
        RepositoryResult<Timetable> Save(Timetable timetable);
    }
}
