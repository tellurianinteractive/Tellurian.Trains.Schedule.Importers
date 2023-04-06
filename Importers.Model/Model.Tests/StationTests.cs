using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TimetablePlanning.Importers.Model.Tests;

[TestClass]
public class StationTests
{
    [TestMethod]
    public void PropertiesAreSet()
    {
        var target = new Station("Stora Höga", "Sth");
        Assert.AreEqual("Stora Höga", target.Name);
        Assert.AreEqual("Sth", target.Signature);
    }

    [TestMethod]
    public void EqualsWorks()
    {
        var s1 = TestDataFactory.CreateStation1();
        var s2 = TestDataFactory.CreateStation1();
        Assert.AreNotSame(s1, s2);
        Assert.AreEqual(s1, s2);
    }
}
