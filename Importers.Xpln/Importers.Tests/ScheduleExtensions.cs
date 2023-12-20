using System.Data.Odbc;
using TimetablePlanning.Importers.Model;

namespace TimetablePlanning.Importers.Xpln.Tests;
internal static class ScheduleExtensions
{
    public static void SaveToDatabase(this Schedule me, int layoutId, string connectionString)
    {
        me.SaveTrains(layoutId, connectionString);
    }

    private static void SaveTrains(this Schedule me, int layoutId, string connectionString)
    {
        using var connection = new OdbcConnection(connectionString);
        var stations = GetStations(layoutId, connectionString);
        var tracks = stations.SelectMany(s => s.Tracks).ToArray();
        connection.Open();
        foreach (var train in me.Timetable.Trains)
        {
            var trainSql = $"INSERT INTO [Train] ([Layout], [Operator], [Number], [OperatingDays], [Category]) VALUES ({layoutId}, 'DSB', {train.Number}, 8, 1)";
            var saveTrainsCommand = new OdbcCommand(trainSql) { Connection = connection };
            saveTrainsCommand.ExecuteNonQuery();
            var getTrainCommand = new OdbcCommand($"SELECT Id FROM TRAIN WHERE Layout = {layoutId} AND Number = {train.Number} ") { Connection = connection };
            var trainId = (int?)getTrainCommand.ExecuteScalar();
            if (trainId.HasValue)
            {
                int callNumber = 0;
                var callsCount = train.Calls.Count;
                foreach (var call in train.Calls)
                {
                    callNumber++;
                    var station = stations.Single(s => s.Signature.Equals(call.Station.Signature, System.StringComparison.OrdinalIgnoreCase));
                    if (station is not null)
                    {
                        var track = station.Tracks.SingleOrDefault(t => t.Number == call.Track.Number);
                        if (track is not null)
                        {
                            var callSql = "INSERT INTO TrainStationCall (IsTrain, IsStationTrack, ArrivalTime, DepartureTime, IsStop, HideArrival, HideDeparture) VALUES " +
                                $"({trainId}, {track.Id}, '{call.Arrival}', '{call.Departure}', -1, {(callNumber == 1 ? -1 : 0)}, {(callNumber == callsCount ? -1 : 0)} )";
                            var saveCallCommand = new OdbcCommand(callSql) { Connection = connection };
                            saveCallCommand.ExecuteNonQuery();
                        }
                    }
                }
            }

        }

    }

    private static List<Station> GetStations(int layoutId, string connectionString)
    {
        using var connection = new OdbcConnection(connectionString);
        var command = new OdbcCommand($"SELECT * FROM XplnGetStations WHERE LayoutId = {layoutId};")
        {
            Connection = connection
        };
        command.Connection.Open();
        var reader = command.ExecuteReader();
        var result = new List<Station>();
        Station? station = null;
        var lastStationId = 0;
        while (reader.Read())
        {
            var stationId = reader.GetInt32(reader.GetOrdinal("StationId"));
            if (station is not null)
            {
                if (stationId != lastStationId)
                {
                    result.Add(station);
                    station = new Station() { Id = stationId, Signature = reader.GetString(reader.GetOrdinal("Signature")) };
                }
            }
            else
            {
                station = new Station() { Id = stationId, Signature = reader.GetString(reader.GetOrdinal("Signature")) };

            }
            var track = new StationTrack(reader.GetString(reader.GetOrdinal("TrackNumber")), true, true);
            track.SetId(reader.GetInt32(reader.GetOrdinal("TrackId")));
            station.Add(track);
            lastStationId = stationId;

        }
        if (station != null) result.Add(station);
        return result;
    }
}
