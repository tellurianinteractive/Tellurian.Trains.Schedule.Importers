using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;


namespace TimetablePlanning.Importers.Model.Tests;

[TestClass]
public class TimetableStretchTests
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    private TimetableStretch Target { get; set; }

    [TestInitialize]
    public void TestInitialize()
    {
        TestDataFactory.Init();
        Target = new TimetableStretch("10", "Ten");
    }

    [ExpectedException(typeof(ArgumentNullException))]
    [TestMethod]
    public void NullNumberThrows()
    {
        Target = new TimetableStretch(null);
    }

    [TestMethod]
    public void PropertiesAreSet()
    {
        Assert.AreEqual("10", Target.Number);
        Assert.AreEqual("Ten", Target.Description);
    }

    [TestMethod]
    public void EqualsWithSameNumber()
    {
        var other = new TimetableStretch("10");
        Assert.AreEqual(Target, other);
    }
}
