namespace Tellurian.Trains.Repositories.Access
{
    using System.Data;
    using System.Data.Odbc;
    using System.Globalization;
    using Tellurian.Trains.Models.Planning;

    internal class StationData
    {
        public long Id { get; set; }
        public string FullName { get; set; }
        public string Signature { get; set; }
        public override string ToString() => $"{Id} {Signature} {FullName}";
    }

    internal static class Stations
    {
        public static IDbCommand CreateGetIdCommand(string stationSignatureOrName)
        {
            var result = new OdbcCommand
            {
                CommandType = CommandType.Text,
                CommandText = $"SELECT Id FROM Station WHERE [Signature] = '{stationSignatureOrName}' OR [FullName] = '{stationSignatureOrName}'"
            };
            return result;
        }

        public static IDbCommand CreateSelectAllCommand()
        {
            var result = new OdbcCommand
            {
                CommandType = CommandType.Text,
                CommandText = "SELECT Id, FullName, Signature, Owner, IsShadow FROM Station"
            };
            return result;
        }

        public static IDbCommand CreateInsertCommand(Station station)
        {
            var result = new OdbcCommand
            {
                CommandType = CommandType.Text,
                CommandText = string.Format(CultureInfo.InvariantCulture,
                $"INSERT INTO Station (FullName, Signature) VALUES ('{station.Name}', '{station.Signature}')")
            };
            return result;
        }

        public static bool AddOrUpdateStation(int layoutId, Station station, IDbConnection connection)
        {
            var getStationIdSql = $"SELECT Id FROM Station WHERE FullName = '{station.Name}'";
            var stationId = (int?)AccessRepository.ExecuteScalar(connection, getStationIdSql);
            if (!stationId.HasValue)
            {
                AccessRepository.ExecuteNonQuery(connection, CreateInsertCommand(station));
                stationId = (int)AccessRepository.ExecuteScalar(connection, getStationIdSql);
                foreach(var track in station.Tracks)
                {
                    using var command = StationTracks.CreateInsertCommand(stationId.Value, track);
                    AccessRepository.ExecuteNonQuery(connection, command);
                }
            }
            var getLayoutStationIdSql = "SELECT Id FROM LayoutStation WHERE Layout = " + layoutId + " AND Station = " + stationId.Value;
            var layoutStationId = (int?)AccessRepository.ExecuteScalar(connection, getLayoutStationIdSql);
            if (!layoutStationId.HasValue)
            {
                AccessRepository.ExecuteNonQuery(connection, LayoutStations.CreateInsertCommand(layoutId, stationId.Value));
            }
            return true;
        }

        public static void RecordHandler(IDataRecord record, Layout layout)
        {
            var result = new Station(record.GetString(record.GetOrdinal("FullName")), record.GetString(record.GetOrdinal("Signature")));
            layout.Add(result);
        }
    }
}
