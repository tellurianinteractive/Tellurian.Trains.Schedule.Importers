using System.Globalization;
using System.Text.RegularExpressions;
using TimetablePlanning.Importers.Model;

namespace TimetablePlanning.Importers.Xpln.Extensions
{
    public static partial class StringExtensions
    {
        [GeneratedRegex("\\d+")]
        private static partial Regex TrainNumberRegex();
        public static string TrainNumber(this string me) => TrainNumberRegex().Match(me).Value.TrimStart('0');

        [GeneratedRegex("\\d+")]
        private static partial Regex TrainCategoryRegex();
        public static string TrainCategory(this string me)
        {
            var start = me.IndexOf('.') + 1;
            var length = 0;
            var end = me.IndexOf(' ');
            if (end >= 0)
                length = end - start;
            else
            {
                var re = TrainCategoryRegex();
                Match m = re.Match(me[start..]);
                if (m.Success) length = m.Index;
            }
            return me.Substring(start, length);
        }

        public static Time AsTime(this string value) =>
            TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out var timespan) ? Time.FromTimeSpan(timespan) :
            DateTime.TryParse(value, CultureInfo.InvariantCulture, out var dateTime) ? Time.FromTimeSpan(dateTime.TimeOfDay) :
            Time.FromDays(double.Parse(value.Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture));

        public static bool IsTime(this string? value) =>
            value.HasValue() &&
                (TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out var _) ||
                DateTime.TryParse(value, CultureInfo.InvariantCulture, out var _) ||
                double.TryParse(value.Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture, out var t) && t >= 0.0 && t <= 1.0);


        public static bool IsTrackNumber(this string? value) =>
            value is not null && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _);


        public static bool Is(this string? me, string? value) =>
            me is not null &&
            value is not null &&
            me.Equals(value, StringComparison.OrdinalIgnoreCase);
        public static bool IsAny(this string? me, params string[] values) =>
             me is not null &&
             values is not null &&
             values.Any(o => o.Equals(me, StringComparison.OrdinalIgnoreCase));

        public static string ValueOrEmpty(this string? value) =>
            value is null ? string.Empty : value;

        public static int NumberOrZero(this string? value) =>
            value is null ? 0 : int.TryParse(value, out var number) ? number : 0;

        public static bool IsNumber(this string? value) =>
            int.TryParse(value, out var _) || double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var _);
        
        public static bool IsNumberOrEmpty(this string? value) =>
            value.IsEmpty() || value.IsNumber();

        public static bool IsZeroOrEmpty(this string? value) =>
            value is null || value == "0";

        public static int ToInteger(this string? value) =>
            int.TryParse(value, out var number) ? number : 0;

        public static double ToDouble(this string? value) =>
            double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var number) ? number : 0.0;

        public static bool IsEmpty(this string? value) =>
            string.IsNullOrWhiteSpace(value);

        public static string OrElse(this string? value, string? other) =>
            !string.IsNullOrWhiteSpace(value) ? value :
            !string.IsNullOrWhiteSpace(other) ? other :
            throw new ArgumentException("Must be a non whitespace string.", nameof(other));

        public static bool HasFileExtension(this string? filename, params string[] extensions) =>
            filename is not null &&
            extensions.Any(e => Path.GetExtension(filename).Equals(e, StringComparison.OrdinalIgnoreCase));
    }
}
