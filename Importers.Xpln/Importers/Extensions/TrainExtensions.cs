using System.Globalization;
using TimetablePlanning.Importers.Model;

namespace TimetablePlanning.Importers.Xpln.Extensions;

internal static class TrainExtensions
{
    public static (Maybe<StationCall> call, int index) FindBetweenArrivalAndDeparture(this Train me, string stationSignature, Time time, int rowNumber)
    {
        if (me.TryFindCall(stationSignature, rowNumber, (c) => true, out (Maybe<StationCall> call, int index) result1))
            return result1;
        if (me.TryFindCall(stationSignature, rowNumber, (c) => c.Arrival == time, out (Maybe<StationCall> call, int index) result4))
            return result4;
        else if (me.TryFindCall(stationSignature, rowNumber, (c) => c.Departure == time, out (Maybe<StationCall> call, int index) result5))
        {
            return result5;
        }
        if (me.TryFindCall(stationSignature, rowNumber, (c) => time >= me.Calls.Last().Arrival && c.Equals(me.Calls.Last()), out (Maybe<StationCall> call, int index) result2))
            return result2;
        if (me.TryFindCall(stationSignature, rowNumber, (c) => time <= me.Calls.First().Departure && c.Equals(me.Calls.First()), out (Maybe<StationCall> call, int index) result3))
            return result3;
        else
        {
            me.TryFindCall(stationSignature, rowNumber, (c) => time > c.Arrival && time < c.Departure, out (Maybe<StationCall> call, int index) result6);
            return result6;
        }
    }
    private static bool TryFindCall(this Train me, string stationSignature, int rowNumber, Func<StationCall, bool> compare, out (Maybe<StationCall> call, int index) result)
    {
        var calls = me.Calls.Select((call, index) => (call, index))
            .Where(item => item.call.Station.Signature.Equals(stationSignature, StringComparison.OrdinalIgnoreCase) && compare(item.call))
            .ToArray();
        if (calls.Length == 1)
        {
            result = (new Maybe<StationCall>(calls.First().call), calls.First().index);
            return true;
        }
        else if (calls.Length == 0)
        {
            result = (new Maybe<StationCall>(string.Format(CultureInfo.CurrentCulture, Resources.Strings.TrainHasNoCallsAtStation, rowNumber, me, stationSignature)), -1);
            return false;
        }
        else
        {
            result = (new Maybe<StationCall>(string.Format(CultureInfo.CurrentCulture, Resources.Strings.TrainHasOverlappingTimesAtStation, rowNumber, me, stationSignature)), -1);
            return false;
        }
    }
}
