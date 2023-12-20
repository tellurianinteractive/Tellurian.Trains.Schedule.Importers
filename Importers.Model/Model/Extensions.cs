using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace TimetablePlanning.Importers.Model;

public static class StringExtensions
{
    public static bool HasText([NotNullWhen(true)] this string? text) => !string.IsNullOrWhiteSpace(text);

    public static string TextOrEmpty(this string? textOrEmptyOrNull) => textOrEmptyOrNull.HasText() ? textOrEmptyOrNull : string.Empty;

    public static string TextOrDefault(this string? textOrNullOrWhiteSpace, string defaultText) =>
        HasText(textOrNullOrWhiteSpace) ? textOrNullOrWhiteSpace : defaultText;

    public static string TextOrException(this string? textOrNullOrWhiteSpace, string parameterName, string? nullOrWhiteSpaceMessage = null) =>
        HasText(textOrNullOrWhiteSpace) ? textOrNullOrWhiteSpace : throw new ArgumentNullException(parameterName, nullOrWhiteSpaceMessage);

    public static bool EqualsIgnoreCase(this string? me, string? text) => me is not null && text is not null && me.Equals(text, StringComparison.OrdinalIgnoreCase);
}

public static class ObjectExtensions
{
    public static bool HasValue([NotNullWhen(true)] this object? value) => value is not null;
    public static T ValueOrException<T>([AllowNull] this T? value, string parameterName) where T : class =>
        value ?? throw new ArgumentNullException(parameterName);

    public static T ValueOrException<T>(this T? value, string parameterName, string nullMessage) where T : class =>
        value ?? throw new ArgumentNullException(parameterName, nullMessage);

    public static void IfNotEqualsThrow<T>(this T one, T other, string notEqualsMessage)
    {
        if (one is null || !one.Equals(other)) throw new ArgumentOutOfRangeException(notEqualsMessage);
    }
}
public static class BoolExtensions
{

    public static void IfTrueThrows(this bool condition, string parameterName, string? trueMessage = null)
    { if (condition) throw new ArgumentOutOfRangeException(parameterName, trueMessage); }
}

public static class TimeSpanExtensions
{
    public static string HHMM(this TimeSpan me) => string.Format(CultureInfo.InvariantCulture, "{0:hh\\:mm}", me);
}
