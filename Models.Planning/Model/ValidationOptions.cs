namespace TimetablePlanning.Importers.Model;

public class ValidationOptions
{
    public bool ValidateStationCalls { get; set; } = true;
    public bool ValidateStationTracks { get; set; } = true;
    public bool ValidateStretches { get; set; } = true;
    public bool ValidateTrainSpeed { get; set; } = true;
    public bool ValidateTrainNumbers { get; set; } = true;
    public bool ValidateLocoSchedules { get; set; } = true;
    public bool ValidateTrainsetSchedules { get; set; } = true;
    public bool ValidateDriverDuties { get; set; } = true;
    public double MinTrainSpeedMetersPerClockMinute { get; set; } = 0.3;
    public double MaxTrainSpeedMetersPerClockMinute { get; set; } = 10;
    public int MinMinutesBetweenTrackUsage { get; set; }
}
