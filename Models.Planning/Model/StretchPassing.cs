using System.Globalization;

namespace TimetablePlanning.Importers.Model;

public sealed record StretchPassing
{
    public Train? Train { get; internal set; }
    public StationCall From { get; }
    public StationCall To { get; }
    public bool IsOdd { get; }

    public StretchPassing(StationCall from, StationCall to, bool isOdd)
    {
        From = from.ValueOrException(nameof(from));
        To = to.ValueOrException(nameof(to));
        IsOdd = isOdd;
    }

    public Time Arrival => To.Arrival;
    public Time Departure => From.Departure;

    public override string ToString() =>
        string.Format(CultureInfo.CurrentCulture, "{0}: {1} - {2}: {3}", From.Station.Name, Departure.HHMM(), To.Station.Name, Arrival.HHMM());
}
