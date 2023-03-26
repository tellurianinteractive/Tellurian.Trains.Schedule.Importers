using System.Data;
using System.Data.Odbc;
using TimetablePlanning.Importers.Model;

namespace TimetablePlanning.Importers.Access;

internal static class Trains
{
    private static readonly IDictionary<int, Train?> CachedTrains = new Dictionary<int, Train?>();

    internal static void Add(int layoutId, Train train, AccessRepository repository)
    {
        using var command = CreateInsertCommand(layoutId, train);
        AccessRepository.ExecuteNonQuery(repository.CreateConnection(), command);
        var trainId = (int?)AccessRepository.ExecuteScalar(repository.CreateConnection(), CreateGetIdCommand(layoutId, train));
        if (trainId.HasValue)
        {
            foreach (var call in train.Calls) TrainsStationCalls.Add(trainId.Value, call, repository.CreateConnection());
        }
    }

    public static IDbCommand CreateGetIdCommand(int layoutId, Train train) =>
        new OdbcCommand
        {
            CommandType = CommandType.Text,
            CommandText = $"SELECT Id FROM Train WHERE [Layout] = '{layoutId}' AND [Number] = '{train.Number}'"
        };

    public static IDbCommand CreateSelectCommand(string layoutName) =>
        new OdbcCommand
        {
            CommandType = CommandType.Text,
            CommandText = $"SELECT Signature, TrackNumber, TrainNumber, Product, Operator, ArrivalTime, DepartureTime, IsStop FROM LayoutTrains WHERE LayoutName = '{layoutName}'"
        };

    public static IDbCommand CreateInsertCommand(int layoutId, Train train) =>
        new OdbcCommand
        {
            CommandType = CommandType.Text,
            CommandText = $"INSERT INTO Train ([Layout], [Number], [OperatingDays]) VALUES ({layoutId}, '{train.Number}', 127)"
        };

    public static void RecordHandler(IDataRecord record, Timetable timetable)
    {
        var currentTrain = timetable.Trains.LastOrDefault();
        var key = Environment.CurrentManagedThreadId;
        var number = record.GetString(record.GetOrdinal("TrainNumber"));
        var category = record.GetString(record.GetOrdinal("Product"));
        if (currentTrain == null)
        {
            currentTrain = new Train(number, number) { Category = category };
            CachedTrains[key] = currentTrain;
        }
        if (currentTrain.Number != number)
        {
            timetable.Add(currentTrain);
            currentTrain = new Train(number, number) { Category = category };
            CachedTrains[key] = currentTrain;
        }
        currentTrain.Add(GetCall(record, timetable));
    }

    private static StationCall GetCall(IDataRecord record, Timetable timetable)
    {
        var signature = record.GetString(record.GetOrdinal("Signature"));
        var trackNumber = record.GetString(record.GetOrdinal("TrackNumber"));
        var station = timetable.Layout.Station(signature);
        var stationTrack = station.Value.Track(trackNumber);
        Time arrivalTime;
        Time departureTime;
        var a = record.GetOrdinal("ArrivalTime");
        var d = record.GetOrdinal("DepartureTime");
        if (record.IsDBNull(a) && record.IsDBNull(d)) throw new InvalidDataException($"Train call at {signature} track {trackNumber} is missing both departure and arrival times.");
        if (record.IsDBNull(a))
        {
            var dep = record.GetDateTime(d);
            arrivalTime = Time.FromHourAndMinute(dep.Hour, dep.Minute);
            departureTime = arrivalTime;
        }
        else if (record.IsDBNull(d))
        {
            var arr = record.GetDateTime(a);
            arrivalTime = Time.FromHourAndMinute(arr.Hour, arr.Minute);
            departureTime = arrivalTime;
        }
        else
        {
            var arr = record.GetDateTime(a);
            arrivalTime = Time.FromHourAndMinute(arr.Hour, arr.Minute);
            var dep = record.GetDateTime(d);
            departureTime = Time.FromHourAndMinute(dep.Hour, dep.Minute);
        }
        return new StationCall(stationTrack.Value, arrivalTime, departureTime);
    }

    public static void FinalHandler(Timetable timetable)
    {
        if (CachedTrains.Count > 0)
        {
            var key = Environment.CurrentManagedThreadId;
            var currentTrain = CachedTrains[key];
            if (currentTrain is not null) timetable.Add(currentTrain);
            CachedTrains[key] = null;
        }
    }
}
