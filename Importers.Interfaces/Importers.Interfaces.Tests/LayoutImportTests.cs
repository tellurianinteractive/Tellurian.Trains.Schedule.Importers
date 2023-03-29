using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using TimetablePlanning.Importers.Model;

namespace TimetablePlanning.Importers.Interfaces.Tests;

[TestClass]
public class LayoutImportTests
{
     [TestMethod] public void ImportsTestLayout() {
        TestDataFactory.Init();
        var result = TestDataSourceService.GetLayout("test");
        var layout = result.Item;
        Assert.AreEqual(3, layout.Stations.Count);
        Assert.AreEqual(layout.TrackStretches.First().End, layout.Station("Yb").Value);
    }
}
