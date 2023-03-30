using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TimetablePlanning.Importers.Access.Tests
{
    [TestClass]
    public class ReadDatabaseTests
    {
        [TestMethod]
        public void ReadsLayoutStations()
        {
            var file = new FileInfo(@"Test data\Timetable.accdb");
            var repository = new AccessRepository(file, NullLogger<AccessRepository>.Instance);
            var schedule = repository.ImportSchedule("Grimslöv H0");
            Assert.IsTrue(schedule.IsSuccess);
            Assert.AreEqual(16, schedule.Item.Timetable.Layout.Stations.Count);
            Assert.AreEqual(62, schedule.Item.Timetable.Layout.Stations.Sum(s => s.Tracks.Count));
        }
    }
}
