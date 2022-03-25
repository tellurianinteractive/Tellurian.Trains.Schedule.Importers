using System.Globalization;
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
            Time.FromDays(double.Parse(value.Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture));

        public static bool IsTrackNumber(this string? value) =>
            value is not null && int.TryParse(value,  NumberStyles.Integer, CultureInfo.InvariantCulture, out _);
    }
}
