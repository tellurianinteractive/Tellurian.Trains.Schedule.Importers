using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tellurian.Trains.Repositories.Xpln.Tests
{
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
    }
}
