using System;
using System.Linq;
using Tellurian.Trains.Models.Planning;

namespace Tellurian.Trains.Repositories.Xpln
{
    public static class LayoutExtenstions
    {
        public static Maybe<StationTrack> Track(this Layout me, string stationSignature, string trackNumber)
        {
            var station = me.Station(stationSignature);
            if (station.IsNone) return Maybe<StationTrack>.None;
            var track = station.Value.Tracks.SingleOrDefault(t => t.Number.Equals(trackNumber, StringComparison.OrdinalIgnoreCase)); 
            if (track is null) return Maybe<StationTrack>.None;
            return new Maybe<StationTrack>(track);
        }
    }
}
