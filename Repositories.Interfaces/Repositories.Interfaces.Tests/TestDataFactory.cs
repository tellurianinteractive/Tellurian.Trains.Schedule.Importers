using System.Collections.Generic;
using System.Linq;
using Tellurian.Trains.Models.Planning;

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

namespace Tellurian.Trains.Repositories.Interfaces.Tests
{
    internal static class TestDataFactory
    {
        public static StationTrack CreateStationTrack()
        {
            var station =  new Station("Ytterby", "Yb");
            station.Add(new StationTrack("1"));
            return station.Tracks.First();
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

        public static Train CreateTrainInForwardDirection(string category, string number, Time startTime)
        {
            var stations = Stations.ToArray();
            var train = new Train(number) { Category = category };
            train.Add(new StationCall(stations[0]["3"], startTime, startTime));
            train.Add(new StationCall(stations[1]["2"], startTime.AddMinutes(25), startTime.AddMinutes(30)));
            train.Add(new StationCall(stations[2]["1"], startTime.AddMinutes(55), startTime.AddMinutes(55)));
            return train;
        }

        public static Train CreateTrainInOppositeDirection(string category, string number, Time startTime)
        {
            var stations = Stations.ToArray();
            var train = new Train(number) { Category = category };
            train.Add(new StationCall(stations[2]["2"], startTime, startTime));
            train.Add(new StationCall(stations[1]["1"], startTime.AddMinutes(25), startTime.AddMinutes(30)));
            train.Add(new StationCall(stations[0]["3"], startTime.AddMinutes(55), startTime.AddMinutes(55)));
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
    }
}
