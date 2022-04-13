using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Tellurian.Trains.Models.Planning
{
#pragma warning disable CS8603 // Possible null reference return.
    public static class StringExtensions
    {
        public static bool HasText(this string? text) => !string.IsNullOrWhiteSpace(text);

        public static string TextOrEmpty(this string? textOrEmptyOrNull) => HasText(textOrEmptyOrNull) ? textOrEmptyOrNull : string.Empty;

        public static string TextOrDefault(this string? textOrNullOrWhiteSpace, string defaultText) =>
            HasText(textOrNullOrWhiteSpace) ? textOrNullOrWhiteSpace : defaultText;

        public static string TextOrException(this string? textOrNullOrWhiteSpace, string parameterName) =>
            HasText(textOrNullOrWhiteSpace) ? textOrNullOrWhiteSpace : throw new ArgumentNullException(parameterName);

        public static string TextOrException(this string? textOrNullOrWhiteSpace, string parameterName, string nullOrWhiteSpaceMessage) =>
            HasText(textOrNullOrWhiteSpace) ? textOrNullOrWhiteSpace : throw new ArgumentNullException(parameterName, nullOrWhiteSpaceMessage);

        public static bool EqualsIgnoreCase(this string? me, string? text) => me is not null && text is not null && me.Equals(text, StringComparison.OrdinalIgnoreCase);
    }
#pragma warning restore CS8603 // Possible null reference return.

    public static class ObjectExtensions
    {
        public static bool IsNull(this object? value) => value is null;

        public static bool HasValue([NotNullWhen(true)] this object? value) => !IsNull(value);
        public static T ValueOrException<T>([AllowNull] this T? value, string parameterName) where T : class =>
            value ?? throw new ArgumentNullException(parameterName);

        public static T ValueOrException<T>(this T? value, string parameterName, string nullMessage) where T : class =>
            value ?? throw new ArgumentNullException(parameterName, nullMessage);

        public static void NotEqualsThrow<T>(this T one, T other, string notEqualsMessage)
        {
            if (one is null || !one.Equals(other)) throw new ArgumentOutOfRangeException(notEqualsMessage);
        }

        public static void TrueThrows(this bool condition, string parameterName, string trueMessage) { if (condition) throw new ArgumentOutOfRangeException(parameterName, trueMessage); }
        public static void TrueThrows(this bool condition, string parametername) => TrueThrows(condition, parametername, string.Empty);
    }

    public static class TimeSpanExtensions
    {
        public static string HHMM(this TimeSpan me) => string.Format(CultureInfo.InvariantCulture, "{0:hh\\:mm}", me);
    }
}
