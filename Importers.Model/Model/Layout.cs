using System.Globalization;

namespace TimetablePlanning.Importers.Model;

public sealed record Layout
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public ICollection<Station> Stations { get; init; }
    public ICollection<TrackStretch> TrackStretches { get; init; }
    public ICollection<TimetableStretch> TimetableStretches { get; init; }

    public Layout()
    {
        Stations = new List<Station>();
        TrackStretches = new List<TrackStretch>();
        TimetableStretches = new List<TimetableStretch>();
    }
    public override string ToString() => Name;
}

public static class LayoutStationsExtensions
{
    public static bool HasStation(this Layout me, Station station) => me?.Stations.Any(s => s.Equals(station)) ?? false;
    public static bool HasTrack(this Layout me, StationTrack track) => me?.StationTracks().Any(t => t.Equals(track)) ?? false;



    public static Maybe<Station> Station(this Layout me, string nameOrSignature) =>
       new(me?.Stations.SingleOrDefault(s => s.Signature.Equals(nameOrSignature, StringComparison.OrdinalIgnoreCase) || s.Name.Equals(nameOrSignature, StringComparison.OrdinalIgnoreCase)),
           Resources.Strings.ThereIsNoStationWithNameOrSignature, nameOrSignature);

    public static IEnumerable<StationTrack> StationTracks(this Layout me) => me is null ? Array.Empty<StationTrack>() : me.Stations.SelectMany(s => s.Tracks);

    public static Station Add(this Layout layout, Station station)
    {
        layout = layout.ValueOrException(nameof(layout));
        station = station.ValueOrException(nameof(station));
        if (!layout.HasStation(station))
        {
            station.Layout = layout;
            layout.Stations.Add(station);
        }
        return station;
    }

    public static TrackStretch Add(this Layout layout, TrackStretch stretch)
    {
        layout = layout.ValueOrException(nameof(layout));
        stretch = stretch.ValueOrException(nameof(stretch));
        if (!layout.TrackStretches.Contains(stretch))
        {
            layout.TrackStretches.Add(stretch);
        }
        return stretch;
    }

    public static TrackStretch Add(this Layout layout, string fromStationName, string toStationName, double distance, int tracksCount)
    {
        var fromStation = layout.Stations.Single(s => s.Name == fromStationName);
        var toStation = layout.Stations.Single(s => s.Name == toStationName);
        var trackStretch = new TrackStretch(fromStation, toStation, distance, tracksCount);
        layout.Add(trackStretch);
        return trackStretch;
    }

}

public static class LayoutExtensions
{
    public static bool HasTimetableStretch(this Layout me, string number) => me is not null && me.TimetableStretches.Any(ts => ts.Number.Equals(number, StringComparison.OrdinalIgnoreCase));
    public static Maybe<TimetableStretch> TimetableStretch(this Layout me, string number)
    {
        me = me.ValueOrException(nameof(me));
        return new Maybe<TimetableStretch>(me.TimetableStretches.SingleOrDefault(ts => ts.Number.Equals(number, StringComparison.OrdinalIgnoreCase)));
    }
    public static TimetableStretch Add(this Layout layout, TimetableStretch timetableStretch)
    {
        layout = layout.ValueOrException(nameof(layout));
        timetableStretch = timetableStretch.ValueOrException(nameof(timetableStretch));
        ArgumentNullException.ThrowIfNull(timetableStretch);
        if (!layout.TimetableStretches.Contains(timetableStretch))
        {
            layout.TimetableStretches.Add(timetableStretch);
        }
        return timetableStretch;
    }
}

public static class LayoutTracksExtensions
{
    public static Maybe<TrackStretch> Add(this Layout layout, string fromStationName, string toStationName, double distance, int tracksCount, int speed, int time)
    {
        var from = layout.Station(fromStationName);
        var to = layout.Station(toStationName);
        if (from.HasValue && to.HasValue)
            return new Maybe<TrackStretch>(layout.Add(new TrackStretch(from.Value, to.Value, distance, tracksCount, speed, time)));
        return new Maybe<TrackStretch>($"From {from} to {to}");
    }

    public static Maybe<TrackStretch> TrackStretch(this Layout trackLayout, Station from, Station to)
        => new(trackLayout?.TrackStretches.SingleOrDefault(ts =>
            (ts.Start.Equals(from) && ts.End.Equals(to)) ||
            (ts.Start.Equals(to) && ts.End.Equals(from))),
            string.Format(CultureInfo.CurrentCulture, Resources.Strings.MoreThanOneStretchBetweenStations, from, to));

    public static Maybe<TrackStretch> TrackStretch(this Layout me, string fromStationNameOrSignature, string toStationNameOrSignature)
    {
        me = me.ValueOrException(nameof(me));
        return new Maybe<TrackStretch>(
            me.Between(fromStationNameOrSignature, toStationNameOrSignature).Concat(
            me.Between(toStationNameOrSignature, fromStationNameOrSignature)).SingleOrDefault(),
            string.Format(CultureInfo.CurrentCulture, Resources.Strings.ThereIsNoStretchBetweenStation1AndStation2, fromStationNameOrSignature, toStationNameOrSignature));
    }

    private static IEnumerable<TrackStretch> Between(this Layout me, string fromStationNameOrSignature, string? toStationNameOrSignature = null)
        => me.TrackStretches.Where(ts =>
            (ts.Start.Name.EqualsIgnoreCase(fromStationNameOrSignature)
            || ts.Start.Signature.EqualsIgnoreCase(fromStationNameOrSignature))
            && (ts.End.Name.EqualsIgnoreCase(toStationNameOrSignature)
            || ts.End.Signature.EqualsIgnoreCase(toStationNameOrSignature)));
}
