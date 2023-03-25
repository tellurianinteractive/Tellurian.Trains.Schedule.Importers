using System.Data;
using System.Data.Odbc;
using System.Globalization;
using Tellurian.Trains.Models.Planning;

namespace Tellurian.Trains.Repositories.Access
{
    internal static class Layouts
    {
        public static IDbCommand GetCurrentNameCommand()
        {
            return new OdbcCommand
            {
                CommandType = CommandType.Text,
                CommandText = "SELECT [Name] FROM Layout WHERE IsCurrent = TRUE"
            };
        }

        public static IDbCommand CreateSelectCommand(string layoutName)
        {
            var result = new OdbcCommand
            {
                CommandType = CommandType.Text,
                CommandText = $"SELECT [Name], StartHour, EndHour FROM Layout WHERE [Name] = '{layoutName}'"
            };
            return result;
        }

        public static IDbCommand CreateInsertCommand(Layout layout)
        {
            var result = new OdbcCommand
            {
                CommandType = CommandType.Text,
                CommandText = "INSERT INTO Layout ([Name], StartHour, EndHour) VALUES (@Name)"
            };
            result.Parameters.AddWithValue("@Name", layout.Name);
             return result;
        }

        public static IDbCommand CreateDeleteCommand(string layoutName)
        {
            var result = new OdbcCommand
            {
                CommandType = CommandType.Text,
                CommandText = "DELETE FROM Layout WHERE [Name] = @Name"
            };
            result.Parameters.AddWithValue("@Name", layoutName);
            return result;
        }
    }
}
