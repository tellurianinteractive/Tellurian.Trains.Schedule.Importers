using System.Data;
using System.Data.Odbc;

namespace TimetablePlanning.Importers.Access;

internal static class LayoutStationTracks
{
    public static IDbCommand CreateSelectCommand(string layoutName) =>
        new OdbcCommand
        {
            CommandType = CommandType.Text,
            CommandText = $"SELECT Signature, Number FROM LayoutStationTracks WHERE LayoutName = '{layoutName}'"
        };
}
