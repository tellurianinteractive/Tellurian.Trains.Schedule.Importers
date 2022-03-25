namespace Tellurian.Trains.Repositories.Access
{
    using System.Data;
    using System.Data.Odbc;
    using Tellurian.Trains.Models.Planning;

    internal static class TimetableTrackStretches
    {
        public static IDbCommand CreateGetIdCommand(int layoutId, TrackStretch stretch)
        {
            var result = new OdbcCommand
            {
                CommandType = CommandType.Text,
                CommandText = "SELECT Id FROM TrackStretchId WHERE [Layout] = @LayoutId AND [FromName] = @FromStationName AND [ToName] = @ToStationName"
            };
            result.Parameters.AddWithValue("@LayoutId", layoutId);
            result.Parameters.AddWithValue("@FromStationName", stretch.Start.Name);
            result.Parameters.AddWithValue("@ToStationName", stretch.End.Name);
            return result;
        }

        public static IDbCommand CreateInsertCommand(int timetableStretchId, int trackStretchId, int sequenceNumber)
        {
            var result = new OdbcCommand
            {
                CommandType = CommandType.Text,
                CommandText = "INSERT INTO TimetableTrackStretch ([TimetableStretch], [TrackStretch], [SequenceNumber]) VALUES (@TimetableStretchId, @TrackStretchId, @SequenceNumber)"
            };
            result.Parameters.AddWithValue("@TimetableStretchId", timetableStretchId);
            result.Parameters.AddWithValue("@TrackStretchId", trackStretchId);
            result.Parameters.AddWithValue("@SequenceNumber", sequenceNumber);
            return result;
        }
    }
}
