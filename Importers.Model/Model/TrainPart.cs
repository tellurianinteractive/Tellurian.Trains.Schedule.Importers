using System.Globalization;

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

namespace TimetablePlanning.Importers.Model;

public sealed record TrainPart : IEquatable<TrainPart>
{
    public VehicleSchedule Schedule { get; internal set; }
    public DriverDuty Duty { get; internal set; }

    public int Id { get; init; }
    public StationCall From { get; init; }
    public StationCall To { get; init; }

    public TrainPart(StationCall from, StationCall to)
    {
        From = from.ValueOrException(nameof(from));
        To = to.ValueOrException(nameof(to));
        From.Train.NotEqualsThrow(To.Train, $"Departure {from} is not same train as arrival {to}.");
    }

    public Train Train => From.Train;
    public Time? Departure => From.Departure;
    public Time? Arrival => To.Arrival;

    public bool Equals(TrainPart? other) => other != null && From.Equals(other.From) && To.Equals(other.To);
    public override int GetHashCode() => HashCode.Combine(From.GetHashCode(), To.GetHashCode());
    public override string ToString() => string.Format(CultureInfo.CurrentCulture, "'{0}' {1} {2}->{3} {4}", Train, From.Station, From.Departure.HHMM(), To.Station, To.Arrival.HHMM());
}

public static class TrainPartExtensions
{
    public static TrainPart AsTrainPart(this Train? train, int fromCallIndex, int toCallIndex)
    {
        var t = train.ValueOrException(nameof(train));
        var c = t.Calls.Count;
        (fromCallIndex < 0 || fromCallIndex > c - 2).TrueThrows(nameof(fromCallIndex));
        (toCallIndex <= fromCallIndex || toCallIndex > c - 1).TrueThrows(nameof(toCallIndex));
        var calls = t.Calls.ToArray();
        return new TrainPart(calls[fromCallIndex], calls[toCallIndex]);
    }

    public static bool IsOverlapping(this TrainPart me, IEnumerable<TrainPart> other)
    {
        return other.Any(o => o.Arrival > me.Departure && o.Departure < me.Arrival);
    }
}
