using System;
using System.Linq;
using TimetablePlanning.Importers.Model;

namespace TimetablePlanning.Importers.Interfaces.Tests;

public sealed class TestDataSourceService : IDataSourceService
{
    public ImportResult<Layout> GetLayout(string name) => ImportResult<Layout>.Success(GetTestLayout(name));

    public ImportResult<Layout> Save(Layout layout)
    {
        throw new NotSupportedException();
    }

    private static Layout GetTestLayout(string name)
    {
        var layout = new Layout { Name = name };
        var stations = TestDataFactory.Stations.ToArray();
        foreach (var station in stations) layout.Add(station);
        var layoutStations = layout.Stations.ToArray();
        for (var i = 0; i < stations.Length - 2; i++)
        {
            var stretch = new TrackStretch(layoutStations[i], layoutStations[i + 1], 10, 1, 100, 10);
            layout.Add(stretch);
        }

        return layout;
    }

    public ImportResult<Timetable> GetTimetable(string name) => GetTestTimetable(name);

    private ImportResult<Timetable> GetTestTimetable(string name)
    {
        var layout = GetLayout(name);
        if (layout.IsFailure) return ImportResult<Timetable>.Failure(layout.Messages);
        var timetable = new Timetable(name, layout.Item);
        timetable.Add(TestDataFactory.CreateTrain1());
        timetable.Add(TestDataFactory.CreateTrain2());
        return ImportResult<Timetable>.Success(timetable);
    }

    public ImportResult<Timetable> Save(Timetable timetable)
    {
        throw new NotSupportedException();
    }

    public ImportResult<Schedule> GetSchedule(string name)
    {
        throw new NotSupportedException();
    }

    public ImportResult<Schedule> Save(Schedule schedule)
    {
        throw new NotSupportedException();
    }
}
