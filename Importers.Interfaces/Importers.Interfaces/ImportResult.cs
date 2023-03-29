using System.Text.Json;
using System.Text.Json.Serialization;

namespace TimetablePlanning.Importers.Interfaces;

public readonly struct ImportResult<T>
{
    public static ImportResult<T> Success() => new(Array.Empty<T>(), Array.Empty<string>(), true);
    public static ImportResult<T> Success(T item) => new(new[] { item }, Array.Empty<string>(), true);
    public static ImportResult<T> Success(T item, string message) => new(new[] { item }, new[] { message }, true);
    public static ImportResult<T> Success(T item, IEnumerable<string> messages) => new(new[] { item }, messages, true);
    public static ImportResult<T> Success(IEnumerable<T> items) => new(items, Array.Empty<string>(), true);
    public static ImportResult<T> Success(IEnumerable<T> items, IEnumerable<string> messages) => new(items, messages, true);
    public static ImportResult<T> Success(IEnumerable<T> items, string message) => new(items, new[] { message }, true);
    public static ImportResult<T> Failure(string message) => new (Array.Empty<T>(), new[] { message }, false);
    public static ImportResult<T> Failure(IEnumerable<string> messages) => new(Array.Empty<T>(), messages, false);
    public static ImportResult<T> SuccessIfNoMessagesOtherwiseFailure(IEnumerable<string> messages) => new(Array.Empty<T>(), messages, !messages.Any());

    private ImportResult(IEnumerable<T> items, IEnumerable<string> messages, bool isSuccess)
    {
        Items = items;
        Messages = messages;
        IsSuccess = isSuccess;
    }
    public IEnumerable<T> Items { get; }
    public T Item => Items.First();
    public IEnumerable<string> Messages { get; init; }
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
}

public static class ImportResultExtensions
{
    public static string Json<T>(this ImportResult<T> me)
    {
        return JsonSerializer.Serialize(me, new JsonSerializerOptions { WriteIndented = true, ReferenceHandler = ReferenceHandler.Preserve });
    }

    public static void Write<T>(this ImportResult<T> me)
    {
        File.WriteAllText($"C:\\Temp\\{me.Item}.json", me.Json());
    }
}