using System.Globalization;

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
#pragma warning disable CA1308 // Normalize strings to uppercase
#pragma warning disable CS0649

namespace TimetablePlanning.Importers.Model;

public sealed record Station : IEquatable<Station>
{
    public Layout Layout { get; internal set; }

    public int Id { get; init; }
    public string Name { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Signature { get; init; }
    public bool IsShadow { get; init; }
    public ICollection<StationTrack> Tracks { get; }
    public Station(string name, string signature)
    {
        name = name.TextOrException(nameof(name), string.Format(CultureInfo.CurrentCulture, Resources.Strings.NameOfObjectIsRequired, Resources.Strings.Station.ToLowerInvariant()));
        Name = name.Replace("_", " ", StringComparison.OrdinalIgnoreCase);
        Signature = signature.TextOrException(nameof(signature), string.Format(CultureInfo.CurrentCulture, Resources.Strings.SignatureOfStationIsRequired));
        Tracks = new List<StationTrack>();
    }
    public Station()
    {
        Tracks = new List<StationTrack>();
    }
    public StationTrack this[string number] => Tracks.SingleOrDefault(t => t.Number == number) ?? throw new InvalidOperationException($"Station {Name} has no track '{number}'");
    public bool Equals(Station? other) => Signature.Equals(other?.Signature, StringComparison.OrdinalIgnoreCase);
    public override int GetHashCode() => Signature.GetHashCode(StringComparison.OrdinalIgnoreCase);
    public override string ToString() => Name;
    public static Station Example => new("Ytterby", "Yb");
}

public static class StationExtensions
{
    public static IEnumerable<Train> Trains(this Station? me) =>
        me is null ? Array.Empty<Train>() : me.Calls().Select(c => c.Train).Distinct();

    public static IEnumerable<StationCall> Calls(this Station me) =>
       me is null ? Array.Empty<StationCall>() : me.Tracks.SelectMany(t => t.Calls);
    public static Maybe<StationTrack> Track(this Station? station, string number)
         => new(station?.Tracks.SingleOrDefault(t => t.Number == number),
             string.Format(CultureInfo.CurrentCulture, Resources.Strings.StationHasNotTrackNumber, station?.Name, number));

    public static bool HasTrack(this Station me, string number)
        => me?.Tracks.Any(t => t.Number == number) ?? false;

    public static StationTrack Add(this Station station, StationTrack stationTrack)
    {
        stationTrack = stationTrack.ValueOrException(nameof(stationTrack));
        if (stationTrack == null) throw new ArgumentNullException(nameof(stationTrack));
        stationTrack.Station = station.ValueOrException(nameof(station));
        station.Tracks.Add(stationTrack);
        return stationTrack;
    }
}
