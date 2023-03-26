using System.Data;
using System.Data.Odbc;
using TimetablePlanning.Importers.Model;

namespace TimetablePlanning.Importers.Access;

internal static class TrainsStationCalls
{
    public static bool Add(int trainId, StationCall call, IDbConnection connection)
    {
        using var command = StationTracks.CreateGetIdCommand(call.Track);
        var stationTrackId = (int?)AccessRepository.ExecuteScalar(connection, command);
        if (stationTrackId.HasValue)
        {
            AccessRepository.ExecuteNonQuery(connection, CreateInsertCommand(trainId, stationTrackId.Value, call));
            return true;
        }
        return false;
    }

    public static OdbcCommand CreateInsertCommand(int trainId, int stationTrackId, StationCall call)
    {
        var result = new OdbcCommand
        {
            CommandType = CommandType.Text,
            CommandText = "INSERT INTO TrainStationCall ([IsTrain], [IsStationTrack], [ArrivalTime], [DepartureTime], [IsStop]) VALUES (@1, @2, @3, @4, @5)"
        };
        result.Parameters.AddWithValue("@1", trainId);
        result.Parameters.AddWithValue("@2", stationTrackId);
        result.Parameters.AddWithValue("@3", call.Arrival.Value);
        result.Parameters.AddWithValue("@4", call.Departure.Value);
        result.Parameters.AddWithValue("@5", call.Arrival != call.Departure);
        return result;
    }
}
