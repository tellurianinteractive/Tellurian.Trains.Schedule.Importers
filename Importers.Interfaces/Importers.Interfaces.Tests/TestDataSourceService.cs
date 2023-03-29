﻿using System;
using System.IO;
using System.Linq;
using TimetablePlanning.Importers.Model;

namespace TimetablePlanning.Importers.Interfaces.Tests;

public sealed class TestDataSourceService 
{
    public static ImportResult<Layout> GetLayout(string name) => ImportResult<Layout>.Success(GetTestLayout(name));

    public static ImportResult<Timetable> GetTimetable( string name)
    {
        var layout = GetTestLayout(name);
        return GetTestTimetable(name, layout);
    }

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


    private static ImportResult<Timetable> GetTestTimetable(string name, Layout layout)
    {
        var timetable = new Timetable(name, layout);
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
