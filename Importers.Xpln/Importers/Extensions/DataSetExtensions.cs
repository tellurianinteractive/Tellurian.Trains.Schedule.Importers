using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Tellurian.Trains.Repositories.Xpln.Extensions;
internal static class DataSetExtensions
{
    public static string[] GetRowFields(this DataRow row)
    {
        var items = row.ItemArray;
        if (items is null) return Array.Empty<string>();
        return items.Select(i => i is null ? string.Empty : i.ToString()).ToArray()!;
    }

    public static bool IsEmptyFields(this IEnumerable<string> fields) =>
        fields.All(i => i.IsEmpty());

    public static bool IsBlankRow(this DataRow row) =>
        row.GetRowFields().IsEmptyFields();
}
