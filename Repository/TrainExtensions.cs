using System.Globalization;
using System.Linq;
using Tellurian.Trains.Models.Planning;

namespace Tellurian.Trains.Repositories.Xpln
{
    internal static class TrainExtensions
    {
        public static (Maybe<StationCall> call, int index) FindBetweenArrivlAndBeparture(this Train me, string stationSignature, Time time)
        {
            var result = me.Calls.Select((call, index) => (call, index)).SingleOrDefault(item => item.call.Station.Signature == stationSignature && item.call.Arrival <= time.Value && item.call.Departure >= time.Value);
            if (result.call is null)
            {
                return (new Maybe<StationCall>(string.Format(CultureInfo.CurrentCulture, Resources.Strings.ThereIsNoStationWithSignatureOrName, stationSignature)), -1);
            }
            return ( new Maybe<StationCall>(result.call), result.index);
        }
    }
}
