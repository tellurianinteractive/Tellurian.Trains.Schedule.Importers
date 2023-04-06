namespace TimetablePlanning.Importers.Model;

public record Schedule
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public Timetable Timetable { get; init; }
    public ICollection<LocoSchedule> LocoSchedules { get; }
    public ICollection<TrainsetSchedule> TrainsetSchedules { get; }
    public ICollection<DriverDuty> DriverDuties { get; }

    public static Schedule Create(string name, Timetable timetable) =>
        new(name, timetable) { };

    private Schedule(string name, Timetable timetable)
    {
        Name = name;
        Timetable = timetable;
        LocoSchedules = new List<LocoSchedule>();
        TrainsetSchedules = new List<TrainsetSchedule>();
        DriverDuties = new List<DriverDuty>();
    }
    public override string ToString() => Name;
}

public static class ScheduleExtensions
{
    public static VehicleSchedule AddLocoSchedule(this Schedule me, LocoSchedule locoSchedule)
    {
        me = me.ValueOrException(nameof(me));
        locoSchedule = locoSchedule.ValueOrException(nameof(locoSchedule));
        if (!me.LocoSchedules.Contains(locoSchedule))
        {
            me.LocoSchedules.Add(locoSchedule);
        }
        return locoSchedule;
    }

    public static VehicleSchedule AddTrainsetSchedule(this Schedule me, TrainsetSchedule trainsetSchedule)
    {
        me = me.ValueOrException(nameof(me));
        trainsetSchedule = trainsetSchedule.ValueOrException(nameof(trainsetSchedule));
        if (!me.TrainsetSchedules.Contains(trainsetSchedule))
        {
            me.TrainsetSchedules.Add(trainsetSchedule);
        }
        return trainsetSchedule;
    }

    public static DriverDuty AddDriverDuty(this Schedule schedule, DriverDuty driverDuty)
    {
        schedule = schedule.ValueOrException(nameof(schedule));
        driverDuty = driverDuty.ValueOrException(nameof(driverDuty));
        if (!schedule.DriverDuties.Contains(driverDuty))
        {
            driverDuty.Schedule = schedule;
            schedule.DriverDuties.Add(driverDuty);
        }
        return driverDuty;
    }
}
