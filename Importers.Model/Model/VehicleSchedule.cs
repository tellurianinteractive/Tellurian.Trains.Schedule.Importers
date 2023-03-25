using System;
using System.Collections.Generic;

namespace TimetablePlanning.Importers.Model;

public abstract record VehicleSchedule
{
    public int Id { get; init; }
    public string Number { get; init; } = string.Empty;
    public ICollection<TrainPart> Parts { get; }

    protected VehicleSchedule(string number)
    {
        Number = number;
        Parts = new List<TrainPart>();
    }

    public override string ToString() => Number;
}

public sealed record LocoSchedule : VehicleSchedule
{
    public LocoSchedule(string number): base(number) { }
}
public sealed record TrainsetSchedule : VehicleSchedule
{
    public TrainsetSchedule(string number) : base(number) { }
}

public static class VehicleScheduleExtensions
{
    public static TrainPart? Add(this VehicleSchedule me, TrainPart? part)
    {
        if (me == null || part is null) throw new ArgumentNullException(nameof(part));
        part.Schedule = me;
        if (!me.Parts.Contains(part))
        {
            me.Parts.Add(part);
        }
        return part;
    }
}
