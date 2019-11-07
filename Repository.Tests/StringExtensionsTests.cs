using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tellurian.Trains.Repositories.Xpln.Tests
{
    [TestClass]
    public class StringExtensionsTests
    {
        [TestMethod]
        public void ParsesTrainNumber()
        {
            Assert.AreEqual(1234, "1234".ParseTrainNumber());
            Assert.AreEqual(5814, "GT CL5814".ParseTrainNumber());
            Assert.AreEqual(8318, "GT HCR 8318".ParseTrainNumber());
        }

        [TestMethod]
        public void ParsesTrainCategory()
        {
            Assert.AreEqual("GT", "054738.GT CN54738".ParsesTrainCategory());
            Assert.AreEqual("Snt", "000100.Snt100".ParsesTrainCategory());
        }
    }
}
