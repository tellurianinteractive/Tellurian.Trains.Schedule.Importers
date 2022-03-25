using System.Data;
using System.Data.Odbc;
using System.Globalization;

namespace Tellurian.Trains.Repositories.Tests
{
    internal static class Sql
    {
        internal static IDbConnection CreateConnection(string databaseFilePath)
        {
            const string driver = "{Microsoft Access Driver (*.mdb, *.accdb)}";
            var connectionString = string.Format(CultureInfo.InvariantCulture, "Driver={0};DBQ={1}", driver, databaseFilePath);
            return new OdbcConnection(connectionString);
        }
    }
}

