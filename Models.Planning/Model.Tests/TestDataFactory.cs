using System.Collections.Generic;
using System.Linq;

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

namespace TimetablePlanning.Importers.Model.Tests;

internal static class TestDataFactory
{
    public static StationTrack CreateStationTrack()
    {
        var result = StationTrack.Example;
        result.Station = new Station("Ytterby", "Yb");
        return result;
    }

    public static void Init()
    {
        Stations = new[] { CreateStation1(), CreateStation2(), CreateStation3() };
    }

    public static IEnumerable<Station> Stations;

    internal static Station CreateStation1()
    {
        var station = new Station("Göteborg", "G");
        station.Add(new StationTrack("1"));
        station.Add(new StationTrack("2"));
        station.Add(new StationTrack("3"));
        station.Add(new StationTrack("4"));
        return station;
    }

    private static Station CreateStation2()
    {
        var station = new Station("Ytterby", "Yb");
        station.Add(new StationTrack("1"));
        station.Add(new StationTrack("2"));
        return station;
    }

    private static Station CreateStation3()
    {
        var station = new Station("Stenungsund", "Snu");
        station.Add(new StationTrack("1"));
        station.Add(new StationTrack("2"));
        return station;
    }

    public static IEnumerable<Train> CreateTrains(string category, Time startTime)
    {
        return new[] {
            CreateTrainInForwardDirection(category, "1", startTime)
        };
    }

    public static Train CreateTrainInForwardDirection(string category, string number, Time startTime)
    {
        var stations = Stations.ToArray();
        var train = new Train(number) { Category = category };
        _ = train.Add(new StationCall(stations[0]["3"], startTime, startTime));
        _ = train.Add(new StationCall(stations[1]["2"], startTime.AddMinutes(25), startTime.AddMinutes(30)));
        _ = train.Add(new StationCall(stations[2]["1"], startTime.AddMinutes(55), startTime.AddMinutes(55)));
        return train;
    }

    public static Train CreateTrainInOppositeDirection(string category, string number, Time startTime)
    {
        var stations = Stations.ToArray();
        var train = new Train(number) { Category = category };
        _ = train.Add(new StationCall(stations[2]["2"], startTime, startTime));
        _ = train.Add(new StationCall(stations[1]["1"], startTime.AddMinutes(25), startTime.AddMinutes(30)));
        _ = train.Add(new StationCall(stations[0]["3"], startTime.AddMinutes(55), startTime.AddMinutes(55)));
        return train;
    }

    public static Train CreateTrain1()
    {
        return CreateTrainInForwardDirection("Godståg", "1234", Time.FromHourAndMinute(12, 00));
    }

    public static Train CreateTrain2()
    {
        return CreateTrainInOppositeDirection("Persontåg", "4321", Time.FromHourAndMinute(12, 00));
    }

    public static Timetable CreateTimetable()
    {
        var timetable = new Timetable("Test", Layout());
        timetable.Add(CreateTrain1());
        timetable.Add(CreateTrain2());
        return timetable;
    }

    public static Layout Layout()
    {
        var layout = new Layout { Name = "Test" };
        foreach (var s in Stations) layout.Add(s);
        var stations = layout.Stations.ToArray();
        for (var i = 0; i < stations.Length - 1; i++) layout.Add(new TrackStretch(stations[i], stations[i + 1], 10));
        var stretch = new TimetableStretch("1");
        foreach (var ts in layout.TrackStretches) stretch.AddLast(ts);
        layout.Add(stretch);
        return layout;
    }
}
