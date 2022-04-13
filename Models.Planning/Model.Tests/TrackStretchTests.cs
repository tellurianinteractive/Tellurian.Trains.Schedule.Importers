using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Tellurian.Trains.Models.Planning.Tests
{
    [TestClass]
    public class TrackStretchTests
    {
        [TestMethod]
        public void ReturnsPassings()
        {
            TestDataFactory.Init();
            var target = TestDataFactory.CreateTimetable();
            foreach (var stretch in target.Layout.TrackStretches)
            {
                Assert.AreEqual(2, stretch.Passings.Count());
            }
        }
    }
}
