namespace Tellurian.Trains.Repositories.Access
{
    using System.Data;
    using System.Data.Odbc;
    using System.Globalization;
    using Tellurian.Trains.Models.Planning;

    internal static class TimetableStretches
    {
        public static bool AddTimetableStretches(int layoutId, TimetableStretch timetableStretch, IDbConnection connection)
        {
            var getTimetableStretchIdSql = "SELECT Id FROM TimetableStretch WHERE Layout = " + layoutId + " AND [Number] = '" + timetableStretch.Number + "'";
            var timetableStretchId = (int?)AccessRepository.ExecuteScalar(connection, getTimetableStretchIdSql);
            if (!timetableStretchId.HasValue)
            {
                using var command = CreateInsertCommand(layoutId, timetableStretch);
                AccessRepository.ExecuteNonQuery(connection, command);
                timetableStretchId = (int)AccessRepository.ExecuteScalar(connection, getTimetableStretchIdSql);
                var sequenceNumber = 0;
                foreach (var stretch in timetableStretch.Stretches)
                {
                    sequenceNumber++;
                    var stretchId = (int)AccessRepository.ExecuteScalar(connection, TimetableTrackStretches.CreateGetIdCommand(layoutId, stretch));
                    AccessRepository.ExecuteNonQuery(connection, TimetableTrackStretches.CreateInsertCommand(timetableStretchId.Value, stretchId, sequenceNumber));
                }
            }

            return true;
        }

        public static IDbCommand CreateCommand(string layoutName)
        {
            var result = new OdbcCommand
            {
                CommandType = CommandType.Text,
                CommandText = string.Format(CultureInfo.InvariantCulture,
                "SELECT FromStation, ToStation, Number, Name FROM LayoutTimetableTrackStretches WHERE LayoutName = '@LayoutName'")
            };
            result.Parameters.AddWithValue("@LayoutName", layoutName);
            return result;
        }

        public static IDbCommand CreateInsertCommand(int layoutId, TimetableStretch stretch)
        {
            var result = new OdbcCommand
            {
                CommandType = CommandType.Text,
                CommandText = "INSERT INTO TimetableStretch ([Layout], [Number], [Name]) VALUES (@LayoutId, @Number, @Name)"
            };
            result.Parameters.AddWithValue("@LayoutId", layoutId);
            result.Parameters.AddWithValue("@Number", stretch.Number);
            result.Parameters.AddWithValue("@Name", stretch.Description);
            return result;
        }

        public static void RecordHandler(IDataRecord record, Layout layout)
        {
            var number = record.GetString(record.GetOrdinal("Number"));
            var name = record.GetString(record.GetOrdinal("Name"));
            var fromStationSignature = record.GetString(record.GetOrdinal("FromStation"));
            var toStationSignature = record.GetString(record.GetOrdinal("ToStation"));
            if (!layout.HasTimetableStretch(number)) layout.Add(new TimetableStretch(number, name));
            var currentTimetableStretch = layout.TimetableStretch(number).Value;
            var trackStretch = layout.TrackStretch(fromStationSignature, toStationSignature);
            currentTimetableStretch.AddLast(trackStretch.Value);
        }
    }
}
