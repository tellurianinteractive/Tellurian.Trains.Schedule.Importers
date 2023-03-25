using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Tellurian.Trains.Models.Planning.Tests
{
    [TestClass]
    public class TrackLayoutTests
    {
         [TestMethod] public void LoadsTestLayout() {
            TestDataFactory.Init();
            ILayoutRepository repository = new TestRepository();
            var (item, _) = repository.GetLayout("test");
            var layout = item.Value;
            Assert.AreEqual(3, layout.Stations.Count);
            Assert.AreEqual(layout.TrackStretches.First().End, layout.Station("Yb").Value);
        }
    }
}
