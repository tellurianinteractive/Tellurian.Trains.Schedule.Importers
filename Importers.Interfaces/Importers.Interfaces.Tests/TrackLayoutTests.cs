using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Tellurian.Trains.Models.Planning;

namespace Tellurian.Trains.Repositories.Interfaces.Tests
{
    [TestClass]
    public class TrackLayoutTests
    {
         [TestMethod] public void LoadsTestLayout() {
            TestDataFactory.Init();
            ILayoutStore repository = new TestRepository();
            var result = repository.GetLayout("test");
            var layout = result.Item;
            Assert.AreEqual(3, layout.Stations.Count);
            Assert.AreEqual(layout.TrackStretches.First().End, layout.Station("Yb").Value);
        }
    }
}
