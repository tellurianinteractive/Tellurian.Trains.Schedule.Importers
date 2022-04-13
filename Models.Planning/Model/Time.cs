using System;
using System.Globalization;

namespace Tellurian.Trains.Models.Planning
{
    public struct Time : IComparable<Time?>, IEquatable<Time>
    {
        public static Time FromString(string time)
        {
            if ( TimeSpan.TryParse(time, out var value)) return new(value);
            throw new ArgumentException("Not a valid time.", time);
        }
        public static Time FromTimeSpan(TimeSpan value) => new (value);
        public static Time FromHourAndMinute(int hours, int minutes) => FromDayHourMinute(0, hours, minutes);
        public static Time FromDayHourMinute(int days, int hours, int minutes) => new (new TimeSpan(days, hours, minutes, 0));
        public static Time FromDays(string value) => value is null ? FromDays(0) : FromDays(double.Parse(value.Replace(",", ".", StringComparison.OrdinalIgnoreCase), NumberStyles.Float, CultureInfo.InvariantCulture));
        public static Time FromDays(double days)
        {
            var t = TimeSpan.FromDays(days).Add(TimeSpan.FromMilliseconds(1)); // Vi lägger till en millisekund för att kompensera avrundningsfel i ODS-filen.
            return new Time(new TimeSpan(t.Hours, t.Minutes, 0));
        }

        private Time(TimeSpan value)
        {
            Value = value;
        }

        public TimeSpan Value { get; }

        public double TotalMinutes => Value.TotalMinutes;

        public int CompareTo(Time? other) => other?.Value is null ? int.MinValue : (int)(Value - other.Value.Value).TotalMinutes;

        public static bool operator !=(Time? time1, Time? time2) => time1?.CompareTo(time2) != 0;
        public static bool operator ==(Time? time1, Time? time2) => time1?.CompareTo(time2) == 0;
        public static bool operator <=(Time? time1, Time? time2) => time1?.CompareTo(time2) <= 0;
        public static bool operator >=(Time? time1, Time? time2) => time1?.CompareTo(time2) >= 0;
        public static bool operator <(Time? time1, Time? time2) => time1?.CompareTo(time2) < 0;
        public static bool operator >(Time? time1, Time? time2) => time1?.CompareTo(time2) > 0;

        public bool Equals(Time other) => other.Value == Value;
        public override bool Equals(object? obj) => obj is Time other && Value.Equals(other.Value);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => string.Format(CultureInfo.InvariantCulture, "{0:hh\\:mm}", Value);
    }

    public static class TimeExtensions
    {
        public static string HHMM(this Time me) => string.Format(CultureInfo.InvariantCulture, "{0:hh\\:mm}", me.Value);
        public static int Hours(this Time me) =>  me.Value.Hours;
        public static Time Subtract(this Time time1, Time time2) => Time.FromTimeSpan(time1.Value - time2.Value);
        public static Time AddMinutes(this Time me, int minutes) => Time.FromTimeSpan(me.Value.Add(TimeSpan.FromMinutes(minutes)));
        public static Time AddDays(this Time me, int days) => Time.FromTimeSpan(me.Value.Add(TimeSpan.FromDays(days)));

        public static Time ParseDays(this string value)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException(nameof(value));
            var parts = value.Split(':');
            if (parts.Length == 1)
            {
                return Time.FromDays(double.Parse(value.Replace(',', '.'), CultureInfo.InvariantCulture) + 0.0001); // Excel uses decimal days.
            }
            else
            {
                return Time.FromHourAndMinute(int.Parse(parts[0], CultureInfo.InvariantCulture), int.Parse(parts[1], CultureInfo.InvariantCulture));
            }
        }
    }
}
