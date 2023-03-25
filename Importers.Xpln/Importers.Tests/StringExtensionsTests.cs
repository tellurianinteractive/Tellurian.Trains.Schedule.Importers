using Microsoft.VisualStudio.TestTools.UnitTesting;
using TimetablePlanning.Importers.Xpln.Extensions;

namespace TimetablePlanning.Importers.Xpln.Tests;

[TestClass]
public class StringExtensionsTests
{
    [TestMethod]
    public void ParsesTrainNumber()
    {
        Assert.AreEqual("1234", "1234".TrainNumber());
        Assert.AreEqual("5814", "GT CL5814".TrainNumber());
        Assert.AreEqual("8318", "GT HCR 8318".TrainNumber());
    }

    [TestMethod]
    public void ParsesTrainCategory()
    {
        Assert.AreEqual("GT", "054738.GT CN54738".TrainCategory());
        Assert.AreEqual("Snt", "000100.Snt100".TrainCategory());
    }

    [TestMethod]
    public void IsTime()
    {
        Assert.IsTrue("12:34".IsTime());
        Assert.IsTrue("1899-12-31 12:34:00".IsTime());
        Assert.IsTrue("0,22222222222646".IsTime());
        Assert.IsFalse("12.60".IsTime());
        Assert.IsFalse("X".IsTime());
    }
}

