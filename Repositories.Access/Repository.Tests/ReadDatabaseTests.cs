using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using Tellurian.Trains.Repositories.Access;

namespace Tellurian.Trains.Repositories.Tests
{
    [TestClass]
    public class ReadDatabaseTests
    {
        [TestMethod]
        public void ReadsLayoutStations()
        {
            var repository = new AccessRepository(@"C:\Users\Stefan\OneDrive\Modelljärnväg\Träffar\2020\2020-10 Värnamo\Trafikplanering\Timetable.accdb");
            var layout = repository.GetLayout("Värnamo");
            Assert.IsTrue(layout.IsSuccess);
            Assert.AreEqual(13, layout.Item.Stations.Count);
        }

        [TestMethod]
        public void ReadsLayoutStationTracks()
        {
        }

    }
}
