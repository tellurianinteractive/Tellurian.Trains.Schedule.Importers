using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Runtime.Serialization;

namespace TimetablePlanning.Importers.Model;

[DataContract(IsReference = true)]
public class TrackStretch : IEquatable<TrackStretch>
{
    public TrackStretch(Station start, Station end, double distance) : this(start, end, distance, 1, 100, (int)Math.Round(distance, 0)) { }
    public TrackStretch(Station start, Station end, double distance, int tracksCount) : this(start, end, distance, tracksCount, 100, (int)Math.Round(distance, 0)) { }
    public TrackStretch(Station start, Station end, double distance, int tracksCount, int speed, int time)
    {
        Start = start.ValueOrException(nameof(start));
        End = end.ValueOrException(nameof(end));
        (!Start.Layout.Equals(end.Layout)).IfTrueThrows(nameof(end), $"Both {start} and {end} must be in the same layout.");
        Distance = distance;
        TracksCount = tracksCount;
        Speed = speed;
        Time = time;
        Layout = Start.Layout;
    }

    [DataMember(IsRequired = false, Order = 1, Name = "Id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    private int _Id = 0;

    public int Id => _Id;

    [DataMember(IsRequired = true, Order = 2)]
    public Station Start { get; }

    [DataMember(IsRequired = true, Order = 3)]
    public Station End { get; }

    [DataMember(IsRequired = true, Order = 4)]
    public double Distance { get; }

    [DataMember(IsRequired = true, Order = 4)]
    public int TracksCount { get; }

    [DataMember(IsRequired = true, Order = 5)]
    public int Speed { get; }

    [DataMember(IsRequired = true, Order = 6)]
    public int Time { get; }
    public Layout Layout { get; }

    public IEnumerable<StretchPassing> Passings => this.GetStretchPassings().ToList();

    public bool Equals(TrackStretch? other) => other != null && Start.Equals(other.Start) && End.Equals(other.End);
    public override bool Equals(object? obj) => obj is TrackStretch other && Equals(other);
    public override int GetHashCode() => Start.GetHashCode() ^ End.GetHashCode();
    public override string ToString() => string.Format(CultureInfo.CurrentCulture, Resources.Strings.StretchToString, Start, End);
}

public static class TrackStretchExtensions
{
    internal static IEnumerable<StretchPassing> GetStretchPassings(this TrackStretch me)
    {
        var trains = me.Start.Trains().Intersect(me.End.Trains());
        var result = new List<StretchPassing>(trains.Count());
        foreach (var train in trains)
        {
            var calls = train.Calls.ToArray();
            for (int i = 0; i < calls.Length - 1; i++)
            {
                if (calls[i].Station.Equals(me.Start) && calls[i + 1].Station.Equals(me.End)) result.Add(new StretchPassing(calls[i], calls[i + 1], true));
                if (calls[i].Station.Equals(me.End) && calls[i + 1].Station.Equals(me.Start)) result.Add(new StretchPassing(calls[i], calls[i + 1], false));
            }
        }
        return result;
    }
}

