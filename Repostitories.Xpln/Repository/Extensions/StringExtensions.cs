using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Tellurian.Trains.Models.Planning;

namespace Tellurian.Trains.Repositories.Xpln
{
    public static class StringExtensions
    {
        public static string TrainNumber(this string me) => Regex.Match(me, @"\d+").Value.TrimStart('0');

        public static string TrainCategory(this string me)
        {
            var start = me.IndexOf(".") + 1;
            var length = 0;
            var end = me.IndexOf(" ");
            if (end >= 0)
            {
                length = end - start;
            }
            else
            {
                Regex re = new Regex(@"\d+");
                Match m = re.Match(me[start..]);
                if (m.Success) length = m.Index;
            }
            return me.Substring(start, length);
        }

        public static Time AsTime(this string value) =>
            TimeSpan.TryParse(value, out var timespan) ? Time.FromTimeSpan(timespan) :
            DateTime.TryParse(value, out var dateTime) ? Time.FromTimeSpan(dateTime.TimeOfDay) :
            Time.FromDays(double.Parse(value.Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture));

        public static bool IsTime(this string? value) =>
            value.HasValue() && (TimeSpan.TryParse(value, out var _) || DateTime.TryParse(value, out var _));


        public static bool IsTrackNumber(this string? value) =>
            value is not null && int.TryParse(value,  NumberStyles.Integer, CultureInfo.InvariantCulture, out _);
    
        public static bool Is(this string? me, string? value) =>
            me is not null && 
            value is not null && 
            me.Equals(value, System.StringComparison.OrdinalIgnoreCase);
        public static bool Is(this string? me, params string[] values) =>
             me is not null &&
             values is not null &&
             values.Any(o => o.Equals(me, System.StringComparison.OrdinalIgnoreCase));

        public static string ValueOrEmpty(this string? value) =>
            value is null ? string.Empty : value;

        public static int NumberOrZero(this string? value) =>
            value is null ? 0 : int.TryParse(value, out var number) ? number : 0;

        public static bool IsNumber(this string? value) =>
            int.TryParse(value, out var _);

        public static bool IsEmpty(this string? value) =>
            string.IsNullOrWhiteSpace(value);

        public static string OrElse(this string? value, string? other) => 
            !string.IsNullOrWhiteSpace(value) ? value :
            !string.IsNullOrWhiteSpace(other) ? other :
            throw new ArgumentException("Must be a non whitespace string.", nameof(other));

        public static bool HasFileExtension(this string? filename, params string[] extensions) =>
            filename is not null &&
            extensions.Any(e => Path.GetExtension(filename).Equals(e, System.StringComparison.OrdinalIgnoreCase));
    }
}
