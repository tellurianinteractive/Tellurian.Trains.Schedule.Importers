namespace Tellurian.Trains.Repositories.Access
{
    using System.Data;
    using System.Data.Odbc;
    using System.Globalization;
    using Tellurian.Trains.Models.Planning;

    internal static class TrackStretches
    {
        internal static void AddTrackStretches(int layoutId, TrackStretch stretch, IDbConnection connection)
        {
            using var command1 = AccessRepository.CreateCommand("SELECT Id FROM Station WHERE [FullName] = '" + stretch.Start.Name + "'");
            using var command2 = AccessRepository.CreateCommand("SELECT Id FROM Station WHERE [FullName] = '" + stretch.End.Name + "'");
            var fromStationId = (int)AccessRepository.ExecuteScalar(connection, command1);
            var toStationId = (int)AccessRepository.ExecuteScalar(connection, command2);
            AccessRepository.ExecuteNonQuery(connection, CreateInsertCommand(layoutId, fromStationId, toStationId));
        }

        public static IDbCommand CreateSelectCommand(string layoutName) =>
            new OdbcCommand
            {
                CommandType = CommandType.Text,
                CommandText = $"SELECT FromStation, ToStation, Distance, TracksCount FROM LayoutTrackStretches WHERE Name = '{layoutName}'"
            };

        public static IDbCommand CreateInsertCommand(int layoutId, int fromStationId, int toStationId)
        {
            var result = new OdbcCommand
            {
                CommandType = CommandType.Text,
                CommandText = string.Format(CultureInfo.InvariantCulture,
                "INSERT INTO TrackStretch ([Layout], [FromStation], [ToStation]) VALUES (@LayoutId, @FromStationId, @ToStationId)")
            };
            result.Parameters.AddWithValue("@LayoutId", layoutId);
            result.Parameters.AddWithValue("@FromStationId", fromStationId);
            result.Parameters.AddWithValue("@ToStationId", toStationId);
            return result;
        }

        public static void RecordHandler(IDataRecord record, Layout layout)
        {
            var from = record.GetString(record.GetOrdinal("FromStation"));
            var to = record.GetString(record.GetOrdinal("ToStation"));
            layout.Add(from, to, record.GetDouble(record.GetOrdinal("Distance")), record.GetInt32(record.GetOrdinal("TracksCount")));
        }
    }
}
