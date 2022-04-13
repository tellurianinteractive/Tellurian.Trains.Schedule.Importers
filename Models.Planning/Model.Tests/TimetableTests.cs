using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Tellurian.Trains.Models.Planning.Tests
{
    [TestClass]
    public class TimetableTests
    {
        [TestMethod]
        public void LoadsTestTimetable()
        {
            TestDataFactory.Init();
            ITimetableReadStore repository = new TestRepository();
            var (item, _) = repository.GetTimetable("scenario1");
            var timetable = item.Value;
            Assert.AreEqual(2, timetable.Trains.Count);
            Assert.AreEqual(3, timetable.Stations.Count());
        }
    }
}
