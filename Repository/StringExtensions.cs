using System.Text.RegularExpressions;

namespace Tellurian.Trains.Repositories.Xpln
{
    public static class StringExtensions
    {
        public static string ParseTrainNumber(this string me) => Regex.Match(me, @"\d+").Value;

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
