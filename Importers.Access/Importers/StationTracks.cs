using System.Data;
using System.Data.Odbc;
using TimetablePlanning.Importers.Model;

namespace TimetablePlanning.Importers.Access;

internal static class StationTracks
{
    public static IDbCommand CreateGetIdCommand(StationTrack track)  =>
        new OdbcCommand
        {
            CommandType = CommandType.Text,
            CommandText = $"SELECT ST.Id FROM StationTrack AS ST INNER JOIN Station AS S ON S.Id = ST.IsAtStation WHERE S.FullName = '{track.Station.Name}' AND ST.Designation = '{track.Number}'"
        };

    public static IDbCommand CreateSelectCommand(string layoutName) =>
        new OdbcCommand
        {
            CommandType = CommandType.Text,
            CommandText = $"SELECT * FROM LayoutStationTracks WHERE LayoutName = '{layoutName}'"
        };

    public static IDbCommand CreateInsertCommand(int stationId, StationTrack track)
    {
        var result = new OdbcCommand
        {
            CommandType = CommandType.Text,
            CommandText = "INSERT INTO StationTrack (IsAtStation, Designation, IsScheduledTrack) VALUES (@StationId, @Designation, @IsScheduledTrack)"
        };
        result.Parameters.AddWithValue("@StationId", stationId);
        result.Parameters.AddWithValue("@Designation", track.Number);
        result.Parameters.AddWithValue("@IsScheduledTrack", track.IsScheduled);
        return result;
    }

    public static void RecordHandler(IDataRecord record, Layout layout)
    {
        var station = layout.Station(record.GetString(record.GetOrdinal("Signature")));
        var track = new StationTrack(record.GetString(record.GetOrdinal("Number")));
        station.Value.Add(track);
    }
}
