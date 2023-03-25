using System;
using System.Collections.Generic;
using System.Linq;

namespace TimetablePlanning.Importers.Model;

public sealed record Timetable
{
    public Layout Layout { get; init; }
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public ICollection<Train> Trains { get; }

    public Timetable(string name, Layout layout)
    {
        Name = name;
        Layout = layout;
        Trains = new List<Train>();
    }

    public override string ToString() => Name;
}

public static class TimetableExtensions
{
    public static int StartHour(this Timetable me) =>
        (me?.Trains.Select(t => t.Calls.Min(c => c.Arrival)).Min(tt => tt).Hours()) ?? 0;
    public static int EndHour(this Timetable me) =>
        (me?.Trains.Select(t => t.Calls.Max(c => c.Arrival)).Max(tt => tt).Hours()+1) ?? 24;

    public static IEnumerable<Station> Stations(this Timetable me) =>
        me is null ? Array.Empty<Station>() : me.Layout.Stations;

    public static Maybe<Train> Train(this Timetable me, string externalId) =>
        new(me?.Trains.Where(t => t.ExtenalId == externalId), $"Train with external id '{externalId}' not found.");

    public static Train Add(this Timetable timetable, Train train)
    {
        timetable = timetable.ValueOrException(nameof(timetable));
        train = train.ValueOrException(nameof(train));
        if (!timetable.Trains.Contains(train))
        {
            train.Timetable = timetable;
            timetable.Trains.Add(train);
        }
        return train;
    }
}
