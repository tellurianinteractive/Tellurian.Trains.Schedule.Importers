using System;
using System.Globalization;
using System.Linq;
using Tellurian.Trains.Models.Planning;

namespace Tellurian.Trains.Repositories.Xpln
{
    internal static class TrainExtensions
    {
        public static (Maybe<StationCall> call, int index) FindBetweenArrivalAndDeparture(this Train me, string stationSignature, Time time)
        {
            if (me.TryFindCall(stationSignature, (c) => c.Arrival == time, out (Maybe<StationCall> call, int index) result1))
            {
                return result1;
            }
            else if (me.TryFindCall(stationSignature, (c) => c.Departure == time, out (Maybe<StationCall> call, int index) result2))
            {
                return result2;
            }
            else
            {
                me.TryFindCall(stationSignature, (c) => time > c.Arrival && time < c.Departure, out (Maybe<StationCall> call, int index) result3);
                return result3;
            }
        }
        private static bool TryFindCall(this Train me, string stationSignature, Func<StationCall, bool> compare, out (Maybe<StationCall> call, int index) result)
        {
            var x = me.Calls.Select((call, index) => (call, index))
                .Where(item => item.call.Station.Signature.Equals(stationSignature, StringComparison.OrdinalIgnoreCase) && compare(item.call));
            if (x.Count() == 1)
            {
                result = (new Maybe<StationCall>(x.First().call), x.First().index);
                return true;
            }
            else if (!x.Any())
            {
                result = (new Maybe<StationCall>(string.Format(CultureInfo.CurrentCulture, Resources.Strings.TrainHasNoCallsAtStation, me, stationSignature)), -1);
                return false;

            }
            else
            {
                result = (new Maybe<StationCall>(string.Format(CultureInfo.CurrentCulture, Resources.Strings.TrainHasOverlappingTimesAtStation, me, stationSignature)), -1);
                return false;
            }
        }
    }
}
