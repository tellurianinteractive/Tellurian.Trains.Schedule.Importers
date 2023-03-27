using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;
using TimetablePlanning.Importers.Model;
using TimetablePlanning.Importers.Xpln.DataSetProviders;

namespace TimetablePlanning.Importers.Xpln.Tests;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

[TestClass]
public class XplnImporterTests
{
    const string FileSuffix = ".ods";
    private DirectoryInfo TestDocumentsDirectory;
    private XplnDataImporter Target;
    private readonly ValidationOptions ValidationOptions = new()
    {
        MaxTrainSpeedMetersPerClockMinute = 8.0,
        MinTrainSpeedMetersPerClockMinute = 0.3,
        ValidateDriverDuties = true,
        ValidateLocoSchedules = true,
        ValidateStationCalls = true,
        ValidateStationTracks = true,
        ValidateStretches = true,
        ValidateTrainsetSchedules = true,
        ValidateTrainSpeed = true,
        ValidateTrainNumbers = true,
        
    };

    [TestInitialize]
    public void TestInitialize()
    {
        TestDocumentsDirectory = new DirectoryInfo("Test data");
        var dataSetProvider =
            new OdsDataSetProvider(TestDocumentsDirectory, NullLogger.Instance);
        Target = new XplnDataImporter(dataSetProvider);
    }


    [TestMethod]
    public void ImportsMontan2023()
    {
        TestDocumentImport("Montan2023H0e", "de-DE", 61, 18, 36, 45, 2);
    }
    [TestMethod]
    public void ImportsBarmstedt2022()
    {
        TestDocumentImport("Barmstedt2022", "de-DE", 61, 18, 36, 45, 2);
    }

    [TestMethod]
    public void ImportsKolding2022()
    {
        TestDocumentImport("Kolding2022", "da-DK", 73, 26, 55, 55, 0);
    }

    [TestMethod]
    public void ImportsDreamTrackTimetable2015()
    {
        TestDocumentImport("DreamTrack2015", null, 62, 24, 0, 40, 0);
    }

    [TestMethod]
    public void ImportsKoldingNorge2019()
    {
        TestDocumentImport("KoldingNorge2019", "no-NO", 56, 16, 0, 56, 1);
    }

    [TestMethod]
    public void ImportsHellerup2015()
    {
        TestDocumentImport("Hellerup2015", "da-DK", 60, 24, 57, 20, 0);
    }

    [TestMethod]
    public void ImportsRotebro2015()
    {
        TestDocumentImport("Rotebro2015", "sv-SE", 39, 15, 0, 31, 1);
    }

    [TestMethod]
    public void ImportsRotebro2016()
    {
        TestDocumentImport("Rotebro2016", "sv-SE", 32, 12, 0, 24, 0);
    }

    [TestMethod]
    public void ImportsTimmele2015()
    {
        TestDocumentImport("Timmele2015", "sv-SE", 37, 13, 0, 33, 4);
    }

    [TestMethod]
    public void ImportsVärnamo2016()
    {
        TestDocumentImport("Värnamo2016", "sv-SE", 40, 13, 0, 27, 0);
    }

    [TestMethod]
    public void ImportsKolding202009()
    {
        TestDocumentImport("Kolding202009", "da-DK", 38, 14, 36, 28, 0);
    }

    [TestMethod]
    public void ImportsVärnamo2017()
    {
        TestDocumentImport("Värnamo2017", "sv-SE", 40, 12, 0, 29, 0);
    }

    private void TestDocumentImport(string scheduleName, string? culture, int expectedTrains, int expectedLocos, int expectedTrainsets, int expectedDuties, int expectedValidationErrors)
    {
        if (string.IsNullOrWhiteSpace(scheduleName)) throw new ArgumentNullException(nameof(scheduleName));
        culture ??= "sv-SE";
        CultureInfo.CurrentCulture = new CultureInfo(culture);
        CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture;
        var file = TestDocumentsDirectory.EnumerateFiles(scheduleName + FileSuffix).Single();
        var result = Target.GetSchedule(scheduleName);
        if (result.IsFailure)
        {
            WriteLines(result.Messages, file);
            Assert.Fail("Stopping errors.");
        }

        var timetable = result.Item.Timetable;
        Assert.AreEqual(expectedTrains, timetable.Trains.Count, "Trains");
        Assert.AreEqual(expectedLocos, result.Item.LocoSchedules.Count, "Locos");
        Assert.AreEqual(expectedTrainsets, result.Item.TrainsetSchedules.Count, "Trainsets");
        Assert.AreEqual(expectedDuties, result.Item.DriverDuties.Count, "Duties");

        var validationErrors = result.Item.GetValidationErrors(ValidationOptions);
        WriteLines(result.Messages.Concat(validationErrors.ToStrings()), file);
        Assert.AreEqual(expectedValidationErrors, validationErrors.Count(), "Validation errors");
    }

    private static void WriteLines(IEnumerable<string> messages, FileInfo file)
    {

        using var writer = new StreamWriter(file.FullName.Replace(FileSuffix, "Log.txt"));
        writer.WriteLine($"Validation at {DateTime.Now}");
        foreach (var message in messages) writer.WriteLine(message);
    }
}
