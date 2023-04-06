using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

namespace TimetablePlanning.Importers.Model.Tests;

[TestClass]
public class VehicleScheduleTests
{
    private VehicleSchedule Target { get; set; }

    [TestInitialize]
    public void TestInitialize()
    {
        Target = new LocoSchedule("W1");
    }

    [TestMethod]
    public void ConstructorSetsProperties()
    {
        Assert.AreEqual("W1", Target.Number);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AddsNullTrainPartThrows()
    {
        Target.Add(null);
    }

    [TestMethod]
    public void AddsTrainPart()
    {
        TestDataFactory.Init();
        var train = TestDataFactory.CreateTrains("Persontåg", Time.FromHourAndMinute(12, 00)).First();
        var part = train.AsTrainPart(0, 1);
        Target.Add(part);
        Assert.AreEqual(part, Target.Parts.First());
    }
}
