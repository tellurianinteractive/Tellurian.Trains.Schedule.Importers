using System.Data;

namespace TimetablePlanning.Importers.Xpln.Extensions;
internal static class DataSetExtensions
{
    public static string[] GetRowFields(this DataRow row)
    {
        var items = row.ItemArray;
        if (items is null) return [];
        return items.Select(i => i is null ? string.Empty : i.ToString()).ToArray()!;
    }

    public static bool IsEmptyFields(this IEnumerable<string> fields) =>
        fields.All(i => i.IsEmpty());

    public static bool IsBlankRow(this DataRow row) =>
        row.GetRowFields().IsEmptyFields();
}
