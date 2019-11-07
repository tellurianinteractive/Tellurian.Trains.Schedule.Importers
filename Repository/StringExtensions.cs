using System.Globalization;
using System.Text.RegularExpressions;

namespace Tellurian.Trains.Repositories.Xpln
{
    public static class StringExtensions
    {
        public static int ParseTrainNumber(this string me)
        {
            return int.Parse(Regex.Match(me, @"\d+").Value, NumberStyles.Any);
        }

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
    }
}
