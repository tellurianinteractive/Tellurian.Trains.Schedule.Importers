using System.Data;
using System.Data.Odbc;
using System.Globalization;


namespace TimetablePlanning.Importers.Access.Tests.Extensions;

internal static class IDbConnectionExtensions
{
    public static IDbConnection CreateMicrosoftAccessDbConnection(string databaseFilePath)
    {
        const string driver = "{Microsoft Access Driver (*.mdb, *.accdb)}";
        var connectionString = string.Format(CultureInfo.InvariantCulture, "Driver={0};DBQ={1}", driver, databaseFilePath);
        return new OdbcConnection(connectionString);
    }

    public static IDataReader ExecuteReader(this IDbConnection connection, IDbCommand command)
    {
        command.Connection = connection;
        command.Connection.Open();
        return command.ExecuteReader(CommandBehavior.CloseConnection);
    }

    public static int ExecuteNonQuery(this IDbConnection connection, IDbCommand command)
    {
        command.Connection = connection;
        try
        {
            command.Connection.Open();
            return command.ExecuteNonQuery();
        }
        finally
        {
            connection.Close();
        }
    }

    
    public static object? ExecuteScalar(this IDbConnection connection, string sql)
    {
        using var command = CreateCommand(sql);
        return ExecuteScalar(connection, command);
    }

    internal static object? ExecuteScalar(IDbConnection connection, IDbCommand command)
    {
        command.Connection = connection;
        try
        {
            command.Connection.Open();
            return command.ExecuteScalar();
        }
        finally
        {
            connection.Close();
        }
    }

    private static IDbCommand CreateCommand(string sql)
    {
        return new OdbcCommand
        {
            CommandType = CommandType.Text,
            CommandText = sql,
        };
    }
}
