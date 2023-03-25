using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using TimetablePlanning.Importers.Model;

namespace TimetablePlanning.Importers.Interfaces.Tests;

[TestClass]
public class TimetableImportTests
{
    [TestMethod]
    public void ImportsTestTimetable()
    {
        TestDataFactory.Init();
        ITimetableSource repository = new TestDataSourceService();
        var result = repository.GetTimetable("scenario1");
        var timetable = result.Item;
        Assert.AreEqual(2, timetable.Trains.Count);
        Assert.AreEqual(3, timetable.Stations().Count());
    }
}
