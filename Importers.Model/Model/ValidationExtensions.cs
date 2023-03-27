using System.Diagnostics;
using System.Globalization;

namespace TimetablePlanning.Importers.Model;

public static class ValidationExtensions
{
    public static IEnumerable<Message> GetValidationErrors(this Schedule schedule, ValidationOptions options)
    {
        schedule = schedule.ValueOrException(nameof(schedule));
        options = options.ValueOrException(nameof(options));
        var result = new List<Message>();
        result.AddRange(schedule.Timetable.GetValidationErrors(schedule, options));
        if (options.ValidateLocoSchedules) result.AddRange(schedule.LocoSchedules.SelectMany(l => l.ValidateOverlappingParts()));
        return result;
    }

    public static IEnumerable<Message> GetValidationErrors(this Timetable timetable, Schedule schedule, ValidationOptions options)
    {
        timetable = timetable.ValueOrException(nameof(timetable));
        options = options.ValueOrException(nameof(options));
        var result = new List<Message>();
        result.AddRange(timetable.EnsureStationHasTrack());
        result.AddRange(timetable.Trains.SelectMany(t => t.CheckTrainTimeSequence()));
        if (options.ValidateStationTracks) result.AddRange(timetable.Stations().SelectMany(s => s.Tracks).SelectMany(t => t.GetValidationErrors(schedule.LocoSchedules)));
        if (options.ValidateStationCalls) result.AddRange(timetable.Stations().SelectMany(s => s.Calls()).SelectMany(c => c.GetValidationErrors()));
        if (options.ValidateStretches) result.AddRange(timetable.Layout.TrackStretches.SelectMany(ss => ss.GetValidationErrors()).Distinct());
        if (options.ValidateTrainSpeed) result.AddRange(timetable.CheckTrainSpeed(options.MinTrainSpeedMetersPerClockMinute, options.MaxTrainSpeedMetersPerClockMinute));
        return result;
    }

    #region Station
    internal static IEnumerable<Message> EnsureStationHasTrack(this Timetable me)
    {
        var result = new List<Message>();
        foreach (var train in me.Trains)
        {
            if (!train.Tracks.Any()) Debugger.Break();
            foreach (var track in train.Tracks)
            {
                var t = track;
                if (!me.Layout.HasTrack(t))
                    result.Add(Message.Information(Resources.Strings.TrackInStationReferredInTrainIsNotInLayout, track, track.Station, train));
            }
        }
        return result;
    }

    #endregion

    #region StationTrack
    public static IEnumerable<Message> GetValidationErrors(this StationTrack me, IEnumerable<LocoSchedule> locos) =>
        me is null ? Array.Empty<Message>() :
        me.GetConflicts(locos).Select(c => Message.Information(Resources.Strings.CallAtStationHasConflictsWithOtherCall, c.one.Train, c.one, c.another.Train, c.another));

    private static IEnumerable<(StationCall one, StationCall another)> GetConflicts(this StationTrack me, IEnumerable<LocoSchedule> locos)
    {
        if (me.Calls.Count < 2) return Array.Empty<(StationCall, StationCall)>();
        var result = GetConflicts(me.Calls.First(), me.Calls.Skip(1), locos);
        return result.Distinct();
    }

    private static IEnumerable<(StationCall one, StationCall other)> GetConflicts(this StationCall me, IEnumerable<StationCall> remaining, IEnumerable<LocoSchedule> locos)
    {
        var result = new List<(StationCall, StationCall)>();
        var conflictingWithMe = remaining.Where(r => r.Track.Equals(me.Track) && !r.Train.Equals(me.Train) && r.Arrival > me.Departure && r.Departure < me.Arrival && !locos.HasSameLoco(r, me)).ToList();
        result.AddRange(conflictingWithMe.Select(c => (me, c)));
        if (remaining.Count() > 1) result.AddRange(GetConflicts(remaining.First(), remaining.Skip(1), locos));
        return result;
    }

    internal static (bool, IEnumerable<StationCall>?) GetConflicts(this StationTrack me, StationCall call, IEnumerable<StationCall> withCalls, IEnumerable<LocoSchedule> locos)
    {
        if (me.Calls.Count == 0) return (false, null);
        if (me.Calls.Count == 2)
        {
            if (me.Calls.First().Station.Equals(me.Calls.Last().Station))
                return (false, null);
        }
        var conflictingCalls = withCalls
            .Where(c => !locos.HasSameLoco(call, c) && (
                (call.Departure > c.Arrival && call.Departure <= c.Departure) ||
                (call.Arrival >= c.Arrival && call.Arrival < c.Departure)));
        if (conflictingCalls.Any())
            return (true, conflictingCalls);
        return (false, null);
    }
    #endregion

    #region StationCall
    public static IEnumerable<Message> GetValidationErrors(this StationCall stationCall)
    {
        if (stationCall?.Train.Number == "9991") Debugger.Break();
        stationCall = stationCall.ValueOrException(nameof(stationCall));
        var result = new List<Message>();
        if (stationCall.Arrival > stationCall.Departure)
            result.Add(Message.Information(Resources.Strings.ArrivalIsAfterDeparture, stationCall.Track.Station.Name, stationCall.Arrival.HHMM(), stationCall.Departure.HHMM()));
        return result;
    }
    #endregion

    #region TrackStretch
    internal static IEnumerable<Message> GetValidationErrors(this TrackStretch me)
    {
        var result = new List<Message>();
        var passings = me.Passings.OrderBy(p => p.Departure.Value).ToArray();
        for (var i = 0; i < passings.Length - me.TracksCount; i++)
        {
            var first = passings[i];
            var second = passings[i + me.TracksCount];
            if (first.To.Train.Number != second.To.Train.Number && first.To.Arrival > second.From.Departure)
                result.Add(Message.Information (Resources.Strings.TrainBetweenPassingIsConflictingWithTrainBetweenPassing, first.From.Train.Number, first, second.To.Train.Number, second));
        }
        return result;
    }

    #endregion

    #region Train
    public static IEnumerable<Message> GetValidationErrors(this Train train, ValidationOptions options)
    {
        train = train.ValueOrException(nameof(train));
        options = options.ValueOrException(nameof(options));
        var result = new List<Message>();
        result.AddRange(train.CheckTrainSpeed(options.MinTrainSpeedMetersPerClockMinute, options.MaxTrainSpeedMetersPerClockMinute));
        result.AddRange(train.CheckTrainTimeSequence());
        return result;
    }

    internal static IEnumerable<Message> CheckTrainSpeed(this Timetable me, double minTrainSpeedMetersPerClockMinute, double maxTrainSpeedMetersPerClockMinute)
    {
        var result = new List<Message>();
        foreach (var train in me.Trains)
        {
            result.AddRange(train.CheckTrainSpeed(minTrainSpeedMetersPerClockMinute, maxTrainSpeedMetersPerClockMinute));
        }
        return result;
    }

    private static IEnumerable<Message> CheckTrainSpeed(this Train me, double minTrainSpeedMetersPerClockMinute, double maxTrainSpeedMetersPerClockMinute)
    {
        var result = new List<Message>();
        var calls = me.Calls.ToArray();
        for (var i = 0; i < calls.Length - 2; i++)
        {
            var c1 = calls[i];
            var c2 = calls[i + 1];
            var maybeStretch = me.Layout.TrackStretch(c1.Station, c2.Station);
            if (maybeStretch.HasValue)
            {
                var time = c2.Arrival.Subtract(c1.Departure);
                var length = maybeStretch.Value.Distance < 2 ? 2 : maybeStretch.Value.Distance;
                
                var speed = time.TotalMinutes == 0 ? 0 : length / time.TotalMinutes;
                if (speed == 0) continue;
                if (speed < minTrainSpeedMetersPerClockMinute)
                    result.Add(Message.Information(Resources.Strings.TrainSpeedBetweenCallsIsTooSlow, c1.Train, c1.Station, c1.Departure.HHMM(), c2.Station, c2.Arrival.HHMM(), length));
                if (speed > maxTrainSpeedMetersPerClockMinute)
                    result.Add(Message.Information(Resources.Strings.TrainSpeedBetweenCallsIsTooFast, c1.Train, c1.Station, c1.Departure.HHMM(), c2.Station, c2.Arrival.HHMM(), length));
            }
        }
        return result;
    }

    internal static IEnumerable<Message> CheckTrainTimeSequence(this Train me)
    {
        var result = new List<Message>();
        if (me.Calls.Count < 1)
        {
            result.Add(Message.Information(Resources.Strings.TrainMustHaveMinimumTwoCalls, me));
        }
        else
        {
            var conflicts = me.GetConflicts();
            if (conflicts.Any())
                result.AddRange(conflicts.Select(c => Message.Information(Resources.Strings.TrainHasConflictingCalls, me, c.one, c.another)));
        }
        return result;
    }

    private static IEnumerable<(StationCall one, StationCall another)> GetConflicts(this Train me)
    {
        var result = new List<(StationCall, StationCall)>();
        if (me.Calls.Count == 2 && me.Calls.First().Station.Equals(me.Calls.Last().Station))

        {
            var c1 = me.Calls.First();
            var c2 = me.Calls.Last();
            if (c1.Arrival > c2.Departure) result.Add((c1, c2));
            else if (c1.Arrival > c2.Arrival) result.Add((c1, c2));
            else if (c1.Departure > c2.Arrival) result.Add((c1, c2));
            else if (c1.Departure > c2.Departure) result.Add((c1, c2));

            return result;
        }
        var calls = me.Calls.ToArray();

        for (var i = 0; i < calls.Length - 1; i++)
        {
            var c1 = calls[i];
            var c2 = calls[i + 1];
            if (c2 != null)
            {
                if (c1.Arrival > c2.Departure) result.Add((c1, c2));
                else if (c1.Arrival > c2.Arrival) result.Add((c1, c2));
                else if (c1.Departure > c2.Arrival) result.Add((c1, c2));
                else if (c1.Departure > c2.Departure) result.Add((c1, c2));
                //if (c1.Station.Equals(c2.Station)) result.Add((c1, c2));
            }
        }
        return result;
    }
    #endregion

    #region VehicleSchedule

    private static IEnumerable<Message> ValidateOverlappingParts(this VehicleSchedule me)
    {
        var messages = new List<Message>();
        var parts = me.Parts.ToArray();
        for (var i = 0; i < parts.Length - 1; i++)
        {
            for (var j = i + 1; j < parts.Length; j++)
            {
                var p1 = parts[i];
                var p2 = parts[j];
                if (p1.To.Arrival > p2.From.Departure && p1.From.Departure < p2.To.Arrival) messages.Add(Message.Information(string.Format(CultureInfo.CurrentCulture, Resources.Strings.VehicleScheduleContainsOverlappingTrainParts, me.Number, p1, p2)));
            }
        }
        return messages;
    }
    #endregion

    internal static bool HasSameLoco(this IEnumerable<LocoSchedule> me, StationCall one, StationCall another)
    {
        var foundOne = me.FindLocoSchedule(one);
        var foundOther = me.FindLocoSchedule(another);
        return foundOne is not null && foundOther is not null && foundOne == foundOther;
    }

    internal static VehicleSchedule? FindLocoSchedule(this IEnumerable<LocoSchedule> me, StationCall call)
    {
        if (me == null) return null;
        foreach (var schedule in me)
        {
            foreach (var part in schedule.Parts)
            {
                if (part.ContainsCall(call)) return schedule;
            }
        }
        return null;
    }

    internal static bool ContainsCall(this TrainPart me, StationCall call)
    {
        return me.Train == call.Train && me.From.Departure <= call.Departure && me.To.Arrival >= call.Arrival;
    }
}
