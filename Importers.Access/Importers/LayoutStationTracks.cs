namespace Tellurian.Trains.Repositories
{
    using System.Data;
    using System.Data.Odbc;

    internal static class LayoutStationTracks
    {
        public static IDbCommand CreateSelectCommand(string layoutName)
        {
            var result = new OdbcCommand
            {
                CommandType = CommandType.Text,
                CommandText = $"SELECT Signature, Number FROM LayoutStationTracks WHERE LayoutName = '{layoutName}'"
            };
            return result;
        }
    }
}
