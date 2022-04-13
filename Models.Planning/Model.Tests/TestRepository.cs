using System;
using System.Collections.Generic;
using System.Linq;

namespace Tellurian.Trains.Models.Planning.Tests
{
    public sealed class TestRepository : ILayoutRepository, ITimetableRepository, IScheduleRepository
    {
        public (Maybe<Layout> item, IEnumerable<Message> messages) GetLayout(string name)
        {
            return (new Maybe<Layout>(GetTestLayout(name)), Array.Empty<Message>());
        }

        public IEnumerable<Message> Save(Layout layout)
        {
            throw new NotSupportedException();
        }

        private static Layout GetTestLayout(string name)
        {
            var layout = new Layout(name);
            var stations = TestDataFactory.Stations.ToArray();
            foreach (var station in stations) layout.Add(station);
            var layoutStations = layout.Stations.ToArray();
            for (var i = 0; i < stations.Length - 2; i++) layout.Add(new TrackStretch(layoutStations[i], layoutStations[i + 1], 10));
            return layout;
        }

        public (Maybe<Timetable> item, IEnumerable<Message> messages) GetTimetable(string name)
        {
            return GetTestTimetable(name);
        }

        private (Maybe<Timetable> item, IEnumerable<Message> messages) GetTestTimetable(string name)
        {
            var (item, messages) = GetLayout(name);
            if (item.IsNone) return (new Maybe<Timetable>("Layout does not exist."), messages);
            var result = new Timetable(name, item.Value);
            result.AddTrain(TestDataFactory.CreateTrain1());
            result.AddTrain(TestDataFactory.CreateTrain2());
            return (new Maybe<Timetable>(result), messages);
        }

        public IEnumerable<Message> Save(Timetable timetable)
        {
            throw new NotSupportedException();
        }

        public (Maybe<Schedule> item, IEnumerable<Message> messages) GetSchedule(string name)
        {
            throw new NotSupportedException();
        }

        public IEnumerable<Message> Save(Schedule schedule)
        {
            throw new NotSupportedException();
        }
    }
}
