using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TimetablePlanning.Importers.Access.Tests
{
    [TestClass]
    public class ReadDatabaseTests
    {
        [TestMethod]
        public void ReadsLayoutStations()
        {
            var repository = new AccessRepository(@"Test data\Timetable.accdb");
            var layout = repository.GetLayout("Grimslöv H0");
            Assert.IsTrue(layout.IsSuccess);
            Assert.AreEqual(16, layout.Item.Stations.Count);
            Assert.AreEqual(62, layout.Item.Stations.Sum(s => s.Tracks.Count));
        }
    }
}
