using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Tellurian.Trains.Models.Planning;
using Tellurian.Trains.Repositories.Interfaces;

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

namespace Tellurian.Trains.Repositories.Xpln.Tests
{
    [TestClass]
    public class XplnRepositoryTests
    {
        private XplnRepository Target;

        [TestInitialize]
        public void TestInitialize()
        {
            Target = new XplnRepository(new DirectoryInfo("Test data"));
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Target?.Dispose();
        }

        [TestMethod]
        public void ReadsStations()
        {
            if (Target != null)
            {
                var result = Target.GetLayout("Värnamo2017");
                Assert.IsTrue(result.IsSuccess);
                result.Write();
            }
        }

        [TestMethod]
        public void ReadsTimetable()
        {
            if (Target is null) throw new InvalidOperationException();
            var result = Target.GetTimetable("Värnamo2017");
            Assert.IsTrue(result.IsSuccess);
            result.Write();
        }

        [TestMethod]
        public void ReadsSchedule()
        {
            if (Target is null) throw new InvalidOperationException();
            var result = Target.GetSchedule("Värnamo2017");
            Assert.IsTrue(result.IsSuccess);
            result.Write();
        }

        [TestMethod]
        public void ImportsKolding2022()
        {
            TestDocumentImport("Kolding2022", "da-DK", 73, 26, 6, 54, 0);
        }

        [TestMethod]
        public void ImportsDreamTrackTimetable2015()
        {
            TestDocumentImport("DreamTrack2015", null, 62, 24, 0, 40, 0);
        }

        [TestMethod]
        public void ImportsKoldingNorge2019()
        {
            TestDocumentImport("KoldingNorge2019", "no-NO", 56, 16, 0, 0, 1);
        }

        [TestMethod]
        public void ImportsHellerup2015()
        {
            TestDocumentImport("Hellerup2015", "da-DK", 60, 24, 0, 20, 0);
        }

        [TestMethod]
        public void ImportsRotebro2015()
        {
            TestDocumentImport("Rotebro2015", "sv-SE", 39, 15, 0, 31, 1);
        }

        [TestMethod]
        public void ImportsRotebro2016()
        {
            TestDocumentImport("Rotebro2016", "sv-SE", 32, 12, 0, 24, 33);
        }

        [TestMethod]
        public void ImportsTimmele2015()
        {
            TestDocumentImport("Timmele2015", "sv-SE", 37, 13, 0, 33, 5);
        }

        [TestMethod]
        public void ImportsVärnamo2016()
        {
            TestDocumentImport("Värnamo2016", "sv-SE", 40, 13, 0, 27, 0);
        }

        [TestMethod]
        public void ImportsKolding202009()
        {
            TestDocumentImport("Kolding202009", "da-DK", 38, 12, 2, 28, 0);
        }

        [TestMethod]
        public void ImportsVärnamo2017()
        {
            TestDocumentImport("Värnamo2017", "sv-SE", 40, 12, 0, 29, 0);
        }

        private void TestDocumentImport(string scheduleName, string? culture, int expectedTrains, int expectedLocos, int expectedTrainsets, int expectedDuties, int expectedValidationErrors)
        {
            if (string.IsNullOrWhiteSpace(scheduleName)) throw new ArgumentNullException(nameof(scheduleName));
            if (culture == null) culture = "sv-SE";
            CultureInfo.CurrentCulture = new CultureInfo(culture);
            CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture;
            var file = Target.DocumentsDirectory.EnumerateFiles(scheduleName + ".ods").Single();
            var schedule = Target.GetSchedule(scheduleName);
            if (schedule.IsFailure)
            {
                WriteLines(schedule.Messages, file);
                Assert.Fail("Stopping errors.");
            }

            var timetable = schedule.Item.Timetable;
            Assert.AreEqual(expectedTrains, timetable.Trains.Count, "Trains");
            Assert.AreEqual(expectedLocos, schedule.Item.LocoSchedules.Count, "Locos");
            Assert.AreEqual(expectedTrainsets, schedule.Item.TrainsetSchedules.Count, "Trainsets");
            Assert.AreEqual(expectedDuties, schedule.Item.DriverDuties.Count, "Duties");
            var validationErrors = schedule.Item.GetValidationErrors(new ValidationOptions { ValidateStretches = true, MinTrainSpeedMetersPerClockMinute = 0.5, MaxTrainSpeedMetersPerClockMinute = 2.0 });
            Assert.AreEqual(expectedValidationErrors, validationErrors.Count());
            WriteLines(validationErrors.ToStrings(), file);
            schedule.Write();
        }

        private static void WriteLines(IEnumerable<string> messages, FileInfo file)
        {
            using var writer = new StreamWriter(file.FullName.Replace(".ods", ".txt"));
            foreach (var message in messages) writer.WriteLine(message);
        }
    }
}
