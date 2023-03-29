using System.Data;
using System.Data.Odbc;
using System.Globalization;
using System.Runtime.CompilerServices;
using TimetablePlanning.Importers.Access.Tests.Extensions;
using TimetablePlanning.Importers.Interfaces;
using TimetablePlanning.Importers.Model;

[assembly: InternalsVisibleTo("TimetablePlanning.Importers.Access.Tests")]

namespace TimetablePlanning.Importers.Access;

public class AccessRepository 
{
    private readonly string DatabaseFileName;

    public AccessRepository(string databaseFullPathName)
    {
        DatabaseFileName = databaseFullPathName;
    }

    public ImportResult<Layout> GetLayout(string name)
    {
        var layout = ReadLayout(name);
        if (layout is not null)
        {
            ReadLayoutStations(layout);
            ReadStationTracks(layout);
            ReadTrackStretches(layout);
            ReadTimetableStretches(layout);
            return ImportResult<Layout>.Success(layout);
        }
        return ImportResult<Layout>.Failure(string.Format(CultureInfo.CurrentCulture, Resources.Strings.TrackLayoutDoesNotExist, name));
    }

    private Layout? ReadLayout(string layoutName)
    {
        using var command = Layouts.CreateSelectCommand(layoutName);
        command.Connection = CreateConnection();
        command.Connection.Open();
        using (var reader = command.ExecuteReader(CommandBehavior.CloseConnection))
        {
            if (reader.Read())
            {
                return new Layout { Name = (reader.GetString(0)) };
            }
        }
        command.Connection.Close();
        return null;
    }

    public Timetable GetTimetable(string name)
    {
        var layout = GetLayout(name);
        var timetable = new Timetable(name, layout.Item);
        GetTrains(timetable);
        return timetable;
    }

    private void ReadLayoutStations(Layout layout)
    {
        using var command = LayoutStations.CreateSelectCommand(layout.Name);
        GetData(command, layout, Stations.RecordHandler);
    }

    private void ReadStationTracks(Layout layout)
    {
        using var command = LayoutStationTracks.CreateSelectCommand(layout.Name);
        GetData(command, layout, StationTracks.RecordHandler);
    }

    private void ReadTrackStretches(Layout layout)
    {
        using var command = TrackStretches.CreateSelectCommand(layout.Name);
        GetData(command, layout, TrackStretches.RecordHandler);
    }

    private void ReadTimetableStretches(Layout layout)
    {
        using var command = TimetableStretches.CreateSelectCommand(layout.Name);
        GetData(command, layout, TimetableStretches.RecordHandler);
    }

    private void GetTrains(Timetable timetable)
    {
        using var command = Trains.CreateSelectCommand(timetable.Name);
        GetData(command, timetable, Trains.RecordHandler, Trains.FinalHandler);
    }

    private void GetData<T>(IDbCommand command, T container, Action<IDataRecord, T> itemHandler)
    {
        GetData(command, container, itemHandler, null);
    }

    private void GetData<T>(IDbCommand command, T container, Action<IDataRecord, T> itemHandler, Action<T>? finalHandler)
    {
        using (var connection = CreateConnection())
        {
            var reader = connection.ExecuteReader( command);
            while (reader.Read())
            {
                itemHandler.Invoke(reader, container);
            }
        }
        finalHandler?.Invoke(container);
    }

    public ImportResult<Layout> Save(Layout layout)
    {
        var existing = ReadLayout(layout.Name);
        if (existing is not null)
            return ImportResult<Layout>.Failure(string.Format(CultureInfo.CurrentCulture, "Can only save new track layout, not update existing layout {0}.", layout.Name));
        using var command = Layouts.CreateInsertCommand(layout);
        _ = ExecuteNonQuery(command);

        var layoutId = (int?)ExecuteScalar(CreateCommand("SELECT Id FROM Layout WHERE [Name] = '" + layout.Name + "'"));
        if (layoutId.HasValue)
        {
            foreach (var station in layout.Stations) Stations.AddOrUpdateStation(layoutId.Value, station, CreateConnection());
            foreach (var stretch in layout.TrackStretches) TrackStretches.AddTrackStretches(layoutId.Value, stretch, CreateConnection());
            foreach (var stretch in layout.TimetableStretches) TimetableStretches.AddTimetableStretches(layoutId.Value, stretch, CreateConnection());
            return ImportResult<Layout>.Success();

        }
        return ImportResult<Layout>.Failure(string.Format(CultureInfo.CurrentCulture, "Layout {0} does not exist.", layout.Name));
    }

    public ImportResult<Timetable> Save(Timetable timetable)
    {
        var existing = ReadLayout(timetable.Name);
        if (existing is not null)
            return ImportResult<Timetable>.Failure(string.Format(CultureInfo.CurrentCulture, "Can only save new timetable, not update existing timetable {0}.", timetable.Name));
        Save(timetable.Layout);
        using var command = CreateCommand("SELECT Id FROM Layout WHERE [Name] = @Name", "@Name", timetable.Name);
        var layoutId = (int?)ExecuteScalar(CreateConnection(), command);
        if (layoutId.HasValue)
        {
            foreach (var train in timetable.Trains) Trains.Add(layoutId.Value, train, this);
            return ImportResult<Timetable>.Success();
        }
        return ImportResult<Timetable>.Failure(string.Format(CultureInfo.CurrentCulture, "Layout {0} does not exist.", timetable.Layout.Name));
    }

    internal int Delete(string layoutName)
    {
        using var command = Layouts.CreateDeleteCommand(layoutName);
        return ExecuteNonQuery(CreateConnection(), command);
    }

 
    internal object? ExecuteScalar(IDbCommand command)
    {
        return ExecuteScalar(CreateConnection(), command);
    }

    internal static object? ExecuteScalar(IDbConnection connection, string sql)
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

    internal int ExecuteNonQuery(IDbCommand command)
    {
        return ExecuteNonQuery(CreateConnection(), command);
    }

    internal static int ExecuteNonQuery(IDbConnection connection, IDbCommand command)
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

    internal static IDbCommand CreateCommand(string sql)
    {
        return new OdbcCommand
        {
            CommandType = CommandType.Text,
            CommandText = sql,
        };
    }

    internal static IDbCommand CreateCommand(string sql, string parameterName, string parameterValue)
    {
        var result = CreateCommand(sql);
        result.Parameters.Add(new OdbcParameter(parameterName, parameterValue));
        return result;
    }

    internal IDbConnection CreateConnection() => IDbConnectionExtensions.CreateMicrosoftAccessDbConnection(DatabaseFileName);
}
