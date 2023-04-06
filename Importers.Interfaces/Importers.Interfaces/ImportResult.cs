using System.Text.Json;
using System.Text.Json.Serialization;
using TimetablePlanning.Importers.Model;

namespace TimetablePlanning.Importers.Interfaces;

public readonly struct ImportResult<T>
{
    public static ImportResult<T> Success() => new(Array.Empty<T>(), Array.Empty<Message>(), true);
    public static ImportResult<T> Success(T item) => new(new[] { item }, Array.Empty<Message>(), true);
    public static ImportResult<T> Success(T item, Message message) => new(new[] { item }, new[] { message }, true);
    public static ImportResult<T> Success(T item, IEnumerable<Message> messages) => new(new[] { item }, messages, true);
    public static ImportResult<T> Success(IEnumerable<T> items) => new(items, Array.Empty<Message>(), true);
    public static ImportResult<T> Success(IEnumerable<T> items, IEnumerable<Message> messages) => new(items, messages, true);
    public static ImportResult<T> Success(IEnumerable<T> items, Message message) => new(items, new[] { message }, true);
    public static ImportResult<T> Failure(Message message) => new(Array.Empty<T>(), new[] { message }, false);
    public static ImportResult<T> Failure(IEnumerable<Message> messages) => new(Array.Empty<T>(), messages, false);
    public static ImportResult<T> SuccessIfNoErrorMessagesOtherwiseFailure(T? item, IEnumerable<Message> messages) => new(item is null ? Array.Empty<T>() : new[] { item }, messages, !messages.Any(m => m.Severity > Severity.Warning));

    private ImportResult(IEnumerable<T> items, IEnumerable<Message> messages, bool isSuccess)
    {
        Items = items;
        Messages = messages;
        IsSuccess = isSuccess;
    }
    public IEnumerable<T> Items { get; init; }
    public T Item => Items.First();
    public IEnumerable<Message> Messages { get; init; }
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