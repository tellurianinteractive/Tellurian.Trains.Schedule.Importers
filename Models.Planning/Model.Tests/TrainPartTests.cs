using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

namespace TimetablePlanning.Importers.Model.Tests;

[TestClass]
public class TrainPartTests
{
    public Train Train { get; set; }

    [TestInitialize]
    public void TestInitialize()
    {
        TestDataFactory.Init();
        Train = TestDataFactory.CreateTrain1();
    }

    [TestMethod]
    public void NullTrainThrows()
    {
        Assert.ThrowsException<ArgumentNullException>(() => TrainPartExtensions.AsTrainPart(null, 0, 1));
    }

    [TestMethod]
    public void NegativeStartIndexThrows()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => Train.AsTrainPart(-1, 1));
    }

    [TestMethod]
    public void FromIndexIsLastIndexThrows()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => Train.AsTrainPart(Train.Calls.Count - 1, 1));
    }

    [TestMethod]
    public void ToIndexEqualToStartIndexThrows()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => Train.AsTrainPart(1, 1));
    }

    [TestMethod]
    public void ToIndexIsGreaterThanLastThrows()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => Train.AsTrainPart(1, 3));
    }

    [TestMethod]
    public void FromAndToStationsAreSet()
    {
        var target = Train.AsTrainPart(1, 2);
        Assert.AreEqual(target.From.Station.Name, "Ytterby");
        Assert.AreEqual(target.To.Station.Name, "Stenungsund");
    }

    [TestMethod]
    public void EqualWorks()
    {
        var one = Train.AsTrainPart(1, 2);
        var another = Train.AsTrainPart(1, 2);
        Assert.AreEqual(one, another);
    }
}
