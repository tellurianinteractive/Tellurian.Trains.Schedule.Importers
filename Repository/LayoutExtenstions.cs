using System;
using System.Linq;
using Tellurian.Trains.Models.Planning;

namespace Tellurian.Trains.Repositories.Xpln
{
    public static class LayoutExtenstions
    {
        public static StationExit GetStationExit(this TrackLayout me, string stationSignature, string exitName)
        {
            var station = me.Stations.Where(t => t.Signature == stationSignature).SingleOrDefault();
            if (station == null) throw new ArgumentOutOfRangeException("Station " + stationSignature + " does not exits in layout " + me.Name + ".");
            var result = new StationExit(exitName);
            station.Add(result);
            return result;
        }
    }
}
