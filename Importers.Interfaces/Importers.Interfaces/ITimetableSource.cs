using TimetablePlanning.Importers.Model;

namespace TimetablePlanning.Importers.Interfaces;

public interface ITimetableStore: ITimetableSource, ITimetableWriteStore { }

public interface ITimetableSource
{
    ImportResult<Timetable> GetTimetable(string name);
}

public interface ITimetableWriteStore
{
    ImportResult<Timetable> Save(Timetable timetable);
}
