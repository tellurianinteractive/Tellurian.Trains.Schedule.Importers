using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using Tellurian.Trains.Models.Planning;

namespace Tellurian.Trains.Repositories.Access
{
    internal static class LayoutStations
    {
        public static IDbCommand CreateSelectCommand(string layoutName) =>
           new OdbcCommand
           {
               CommandType = CommandType.Text,
               CommandText = $"SELECT FullName, Signature FROM LayoutStations WHERE LayoutName = '{layoutName}'"
           };

        public static OdbcCommand CreateInsertCommand(int layoutId, int stationId)
        {
            var result = new OdbcCommand
            {
                CommandType = CommandType.Text,
                CommandText = "INSERT INTO LayoutStation (Layout, Station) VALUES (@LayoutId, @StationId)"
            };
            result.Parameters.AddWithValue("@LayoutId", layoutId);
            result.Parameters.AddWithValue("@StationId", stationId);
            return result;
        }

        public static void RecordHandler(IDataRecord record, Layout layout)
        {
            var result = new Station(record.GetString(record.GetOrdinal("FullName")), record.GetString(record.GetOrdinal("Signature")));
            layout.Add(result);
        }
    }

    internal static class LayoutStationExtensions
    {
    }
}
