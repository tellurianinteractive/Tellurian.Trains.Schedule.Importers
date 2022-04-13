using System;
using System.Linq;
using Tellurian.Trains.Models.Planning;
namespace Tellurian.Trains.Repositories.Interfaces.Tests
{
    public sealed class TestRepository : ILayoutStore, ITimetableStore, IScheduleRepository
    {
        public RepositoryResult<Layout> GetLayout(string name) => RepositoryResult<Layout>.Success(GetTestLayout(name));

        public RepositoryResult<Layout> Save(Layout layout)
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

        public RepositoryResult<Timetable> GetTimetable(string name) => GetTestTimetable(name);

        private RepositoryResult<Timetable> GetTestTimetable(string name)
        {
            var layout = GetLayout(name);
            if (layout.IsFailure) return RepositoryResult<Timetable>.Failure(layout.Messages);
            var timetable = new Timetable(name, layout.Item);
            timetable.Add(TestDataFactory.CreateTrain1());
            timetable.Add(TestDataFactory.CreateTrain2());
            return RepositoryResult<Timetable>.Success(timetable);
        }

        public RepositoryResult<Timetable> Save(Timetable timetable)
        {
            throw new NotSupportedException();
        }

        public RepositoryResult<Schedule> GetSchedule(string name)
        {
            throw new NotSupportedException();
        }

        public RepositoryResult<Schedule> Save(Schedule schedule)
        {
            throw new NotSupportedException();
        }
    }
}
