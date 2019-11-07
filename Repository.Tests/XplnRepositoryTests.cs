using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Tellurian.Trains.Models.Planning;

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
            Target.Dispose();
        }

        [TestMethod]
        public void ImportsDreamTrackTimetable2015()
        {
            TestDocumentImport("DreamTrack2015", null, 62, 24, 40, 0);
        }

        [TestMethod]
        public void ImportsKoldingNorge2019()
        {
            TestDocumentImport("KoldingNorge2019", "no-NO", 56, 16, 0, 1);
        }

        [TestMethod]
        public void ImportsHellerup2015()
        {
            TestDocumentImport("Hellerup2015", "da-DK", 60, 24, 3, 0);
        }

        [TestMethod]
        public void ImportsRotebro2015()
        {
            TestDocumentImport("Rotebro2015", "sv-SE", 39, 15, 31, 1);
        }

        [TestMethod]
        public void ImportsRotebro2016()
        {
            TestDocumentImport("Rotebro2016", "sv-SE", 32, 12, 24, 33);
        }

        [TestMethod]
        public void ImportsTimmele2015()
        {
            TestDocumentImport("Timmele2015", "sv-SE", 37, 13, 33, 5);
        }

        [TestMethod]
        public void ImportsVärnamo2016()
        {
            TestDocumentImport("Värnamo2016", "sv-SE", 40, 13, 27, 0);
        }

        [TestMethod]
        public void ImportsVärnamo2017()
        {
            TestDocumentImport("Värnamo2017", "sv-SE", 40, 12, 29, 0);
        }

        private void TestDocumentImport(string scheduleName, string culture, int expectedTrains, int expectedLocos, int expectedDuties, int expectedValidationErrors)
        {
            if (string.IsNullOrWhiteSpace(scheduleName)) throw new ArgumentNullException(nameof(scheduleName));
            if (culture == null) culture = "sv-SE";
            CultureInfo.CurrentCulture = new CultureInfo(culture);
            CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture;
            var file = Target.DocumentsDirectory.EnumerateFiles(scheduleName + ".ods").Single();
            var (schedule, scheduleMessages) = Target.GetSchedule(scheduleName);
            if (scheduleMessages.HasStoppingErrors())
            {
                WriteLines(scheduleMessages, file);
                Assert.Fail("Stopping errors.");
            }

            var timetable = schedule.Value.Timetable;
            Assert.AreEqual(expectedTrains, timetable.Trains.Count, "Trains");
            Assert.AreEqual(expectedLocos, schedule.Value.LocoSchedules.Count, "Locos");
            Assert.AreEqual(expectedDuties, schedule.Value.DriverDuties.Count, "Duties");
            var validationErrors = schedule.Value.GetValidationErrors(new ValidationOptions { ValidateStretches = true, MinTrainSpeedMetersPerClockMinute = 0.5, MaxTrainSpeedMetersPerClockMinute = 2.0 });
            Assert.AreEqual(expectedValidationErrors, validationErrors.Count());
            WriteLines(validationErrors, file);
        }

        private void WriteLines(IEnumerable<Message> messages, FileInfo file)
        {
            using var writer = new StreamWriter(file.FullName.Replace(".ods", ".txt"));
            foreach (var message in messages) writer.WriteLine(message.ToString());
        }
    }
}
