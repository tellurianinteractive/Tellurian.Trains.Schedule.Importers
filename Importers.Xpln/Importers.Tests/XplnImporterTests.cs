using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;
using System.IO.MemoryMappedFiles;
using TimetablePlanning.Importers.Model;
using TimetablePlanning.Importers.Xpln.DataSetProviders;

namespace TimetablePlanning.Importers.Xpln.Tests;

[TestClass]
public class XplnImporterTests
{
    const string FileSuffix = ".ods";
    private DirectoryInfo? TestDocumentsDirectory;
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
    }

    [TestMethod]
    public void ImportsMemoryMappedFile()
    {
        using var m = MemoryMappedFile.CreateFromFile(TestDocumentsDirectory!.FullName + "\\Montan2023H0e.ods");
        var inputStream = m.CreateViewStream();
        var dataSetProvider = new OdsDataSetProvider(NullLogger<OdsDataSetProvider>.Instance);
        using var importer = new XplnDataImporter(inputStream, dataSetProvider, NullLogger<XplnDataImporter>.Instance);
        var result = importer.ImportSchedule("Montan2023H0e");
        if (result.IsFailure)
        {
            Assert.Fail();
        }

    }

    [TestMethod]
    public void ImportsMontan2023()
    {
        Import("Montan2023H0e", "de-DE", 32, 3, 28, 3, 0);
    }

    [DataTestMethod()]
    [DataRow("Barmstedt2022", "de-DE", 61, 18, 36, 45, 2)]
    [DataRow("Kolding2022", "da-DK", 73, 26, 55, 55, 0)]
    [DataRow("Kolding202009", "da-DK", 38, 14, 36, 28, 0)]
    [DataRow("KoldingNorge2019", "no-NO", 56, 16, 0, 56, 1)]
    [DataRow("Värnamo2017", "sv-SE", 40, 12, 0, 29, 0)]
    [DataRow("Värnamo2016", "sv-SE", 40, 13, 0, 27, 0)]
    [DataRow("Rotebro2016", "sv-SE", 32, 12, 0, 24, 0)]
    [DataRow("Rotebro2015", "sv-SE", 39, 15, 0, 31, 1)]
    [DataRow("Timmele2015", "sv-SE", 37, 13, 0, 33, 4)]
    [DataRow("Hellerup2015", "da-DK", 60, 24, 57, 20, 0)]
    [DataRow("DreamTrack2015", null, 62, 24, 0, 40, 0)]
    public void Import(string scheduleName, string? culture, int expectedTrains, int expectedLocos, int expectedTrainsets, int expectedDuties, int expectedValidationErrors)
    {
        if (string.IsNullOrWhiteSpace(scheduleName)) throw new ArgumentNullException(nameof(scheduleName));
        culture ??= "sv-SE";
        CultureInfo.CurrentCulture = new CultureInfo(culture);
        CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture;
        var file = TestDocumentsDirectory!.EnumerateFiles(scheduleName + FileSuffix).Single();
        var dataSetProvider = new OdsDataSetProvider(NullLogger<OdsDataSetProvider>.Instance);
        using var importer = new XplnDataImporter(file, dataSetProvider, NullLogger<XplnDataImporter>.Instance);
        var result = importer.ImportSchedule(scheduleName);
        if (result.IsFailure)
        {
            WriteLines(result.Messages.ToStrings(), file);
            Assert.Fail("Stopping errors.");
        }

        var timetable = result.Item.Timetable;
        Assert.AreEqual(expectedTrains, timetable.Trains.Count, "Trains");
        Assert.AreEqual(expectedLocos, result.Item.LocoSchedules.Count, "Locos");
        Assert.AreEqual(expectedTrainsets, result.Item.TrainsetSchedules.Count, "Trainsets");
        Assert.AreEqual(expectedDuties, result.Item.DriverDuties.Count, "Duties");

        var validationErrors = result.Item.GetValidationErrors(ValidationOptions);
        WriteLines(result.Messages.ToStrings().Concat(validationErrors.ToStrings()), file);
        Assert.AreEqual(expectedValidationErrors, validationErrors.Count(), "Validation errors");
    }

    private static void WriteLines(IEnumerable<string> messages, FileInfo file)
    {

        using var writer = new StreamWriter(file.FullName.Replace(FileSuffix, "Log.txt"));
        writer.WriteLine($"Validation at {DateTime.Now}");
        foreach (var message in messages) writer.WriteLine(message);
        writer.WriteLine("Validation completed.");
    }
}
