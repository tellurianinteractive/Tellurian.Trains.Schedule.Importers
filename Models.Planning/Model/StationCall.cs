using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

namespace Tellurian.Trains.Models.Planning
{
    public sealed record StationCall : IEquatable<StationCall>, IComparable<StationCall>
    {
        internal Train Train { get; set; }

        public int Id { get; init; }
        public Station Station => Track.Station;
        public StationTrack Track { get; init; }
        public Time Arrival { get; init; }
        public Time Departure { get; init; }
        public bool IsArrival { get; set; }
        public bool IsDeparture { get; set; }
        public ICollection<Note> Notes { get; }
        public bool IsStop => IsArrival || IsDeparture; 
        public Time SortTime => IsDeparture ? Departure : Arrival;

        public StationCall(StationTrack track, Time arrival, Time departure)
        {
            Track = track.ValueOrException(nameof(track));
            Track.Add(this);
            Arrival = arrival;
            Departure = departure;
            Notes = new List<Note>();
        }

        public bool Equals(StationCall? other) => 
             other != null &&
             Arrival.Equals(other.Arrival) &&
             Departure.Equals(other.Departure) &&
             Track.Equals(other.Track) &&
             Train?.Equals(other.Train) == true;

        public override int GetHashCode() => HashCode.Combine(Arrival, Departure, Track, Train);

        public override string ToString() =>
            string.Format(CultureInfo.CurrentCulture, Resources.Strings.CallAtStationTrackDuringTimes, Station, Track, Arrival.HHMM(), Departure.HHMM());

        public int CompareTo([AllowNull] StationCall other) =>
            other is null ? 1 : SortTime.CompareTo(other.SortTime);

        public static bool operator <(StationCall? call1, StationCall? call2) => call1?.CompareTo(call2) == -1;
        public static bool operator >(StationCall? call1, StationCall? call2) => call1?.CompareTo(call2) == 1;
        public static bool operator <=(StationCall? call1, StationCall? call2) => call1?.CompareTo(call2) >= 0;
        public static bool operator >=(StationCall? call1, StationCall? call2) => call1?.CompareTo(call2) <= 0;
    }
}
