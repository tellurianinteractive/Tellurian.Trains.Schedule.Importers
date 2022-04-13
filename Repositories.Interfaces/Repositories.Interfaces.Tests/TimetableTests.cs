using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Tellurian.Trains.Models.Planning;

namespace Tellurian.Trains.Repositories.Interfaces.Tests
{
    [TestClass]
    public class TimetableTests
    {
        [TestMethod]
        public void LoadsTestTimetable()
        {
            TestDataFactory.Init();
            ITimetableReadStore repository = new TestRepository();
            var result = repository.GetTimetable("scenario1");
            var timetable = result.Item;
            Assert.AreEqual(2, timetable.Trains.Count);
            Assert.AreEqual(3, timetable.Stations().Count());
        }
    }
}
