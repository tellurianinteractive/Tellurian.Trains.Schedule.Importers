using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tellurian.Trains.Models.Planning;

namespace Tellurian.Trains.Repositories.Xpln
{
    internal static class TrainExtensions
    {
        public static (Maybe<StationCall> call, int index) FindBetweenArrivlAndBeparture(this Train me, string stationSignature, Time time)
        {
            var result = me.Calls.Select((call, index) => (call, index)).SingleOrDefault(item => item.call.Station.Signature == stationSignature && item.call.Arrival <= time && item.call.Departure >= time);
            if (result.call is null)
            {
                return (Maybe<StationCall>.None(string.Format(CultureInfo.CurrentCulture, Resources.Strings.ThereIsNoStationWithSignatureOrName, stationSignature)), -1);
            }
            return (Maybe<StationCall>.Item(result.call), result.index);
        }
    }
}
