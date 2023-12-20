using System.Data;
using System.Data.Odbc;
using TimetablePlanning.Importers.Access.Tests.Extensions;
using TimetablePlanning.Importers.Model;

namespace TimetablePlanning.Importers.Access;

internal class StationData
{
    public long Id { get; set; }
    public required string FullName { get; set; }
    public required string Signature { get; set; }
    public override string ToString() => $"{Id} {Signature} {FullName}";
}

internal static class Stations
{
    public static IDbCommand CreateGetIdCommand(string stationSignatureOrName) =>
        new OdbcCommand
        {
            CommandType = CommandType.Text,
            CommandText = $"SELECT Id FROM Station WHERE [Signature] = '{stationSignatureOrName}' OR [FullName] = '{stationSignatureOrName}'"
        };

    public static IDbCommand CreateSelectAllCommand() =>
        new OdbcCommand
        {
            CommandType = CommandType.Text,
            CommandText = "SELECT Id, FullName, Signature, Owner, IsShadow FROM Station"
        };

    public static IDbCommand CreateInsertCommand(Station station) =>
        new OdbcCommand
        {
            CommandType = CommandType.Text,
            CommandText = $"INSERT INTO Station (FullName, Signature) VALUES ('{station.Name}', '{station.Signature}')"
        };

    public static bool AddOrUpdateStation(int layoutId, Station station, IDbConnection connection)
    {
        var stationId = (int?)OdbcConnectionExtensions.ExecuteScalar(connection, CreateGetIdCommand(station.Name));
        if (!stationId.HasValue)
        {
            AccessRepository.ExecuteNonQuery(connection, CreateInsertCommand(station));
            stationId = (int?)AccessRepository.ExecuteScalar(connection, CreateGetIdCommand(station.Name));
            if (stationId.HasValue)
            {
                foreach (var track in station.Tracks)
                {
                    using var command = StationTracks.CreateInsertCommand(stationId.Value, track);
                    AccessRepository.ExecuteNonQuery(connection, command);
                }
                var getLayoutStationIdSql = $"SELECT Id FROM LayoutStation WHERE Layout = {layoutId} AND Station = {stationId.Value}";
                var layoutStationId = (int?)AccessRepository.ExecuteScalar(connection, getLayoutStationIdSql);
                if (!layoutStationId.HasValue)
                {
                    AccessRepository.ExecuteNonQuery(connection, LayoutStations.CreateInsertCommand(layoutId, stationId.Value));
                }
                return true;
            }
        }
        return false;
    }

    public static void RecordHandler(IDataRecord record, Layout layout)
    {
        var result = new Station(record.GetString(record.GetOrdinal("FullName")), record.GetString(record.GetOrdinal("Signature")));
        layout.Add(result);
    }
}
