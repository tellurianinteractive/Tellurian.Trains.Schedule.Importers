using System.Globalization;
using System.Text.RegularExpressions;
using Tellurian.Trains.Models.Planning;

namespace Tellurian.Trains.Repositories.Xpln
{
    public static class StringExtensions
    {
        public static string ParseTrainNumber(this string me) => Regex.Match(me, @"\d+").Value.TrimStart('0');

        public static string ParsesTrainCategory(this string me)
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
                Match m = re.Match(me.Substring(start));
                if (m.Success) length = m.Index;
            }
            return me.Substring(start, length);
        }

        public static Time AsTime(this string value) =>
            Time.FromDays(double.Parse(value.Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture));
    }
}
