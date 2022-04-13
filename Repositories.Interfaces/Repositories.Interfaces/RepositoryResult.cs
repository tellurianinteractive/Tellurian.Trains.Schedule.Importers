using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tellurian.Trains.Repositories.Interfaces
{
    public struct RepositoryResult<T>
    {
        public static RepositoryResult<T> Success() => new(Array.Empty<T>(), Array.Empty<string>(), true);
        public static RepositoryResult<T> Success(T item) => new(new[] { item }, Array.Empty<string>(), true);
        public static RepositoryResult<T> Success(T item, string message) => new(new[] { item }, new[] { message }, true);
        public static RepositoryResult<T> Success(T item, IEnumerable<string> messages) => new(new[] { item }, messages, true);
        public static RepositoryResult<T> Success(IEnumerable<T> items) => new(items, Array.Empty<string>(), true);
        public static RepositoryResult<T> Success(IEnumerable<T> items, IEnumerable<string> messages) => new(items, messages, true);
        public static RepositoryResult<T> Success(IEnumerable<T> items, string message) => new(items, new[] { message }, true);
        public static RepositoryResult<T> Failure(string message) => new (Array.Empty<T>(), new[] { message }, false);
        public static RepositoryResult<T> Failure(IEnumerable<string> messages) => new(Array.Empty<T>(), messages, false);
        public static RepositoryResult<T> SuccessIfNoMessagesOtherwiseFailure(IEnumerable<string> messages) => new(Array.Empty<T>(), messages, !messages.Any());

        private RepositoryResult(IEnumerable<T> items, IEnumerable<string> messages, bool isSuccess)
        {
            Items = items;
            Messages = messages;
            IsSuccess = isSuccess;
        }
        public IEnumerable<T> Items { get; }
        public T Item => Items.First();
        public IEnumerable<string> Messages { get; }
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
    }

    public static class RepositoryResultExtensions
    {
        public static string Json<T>(this RepositoryResult<T> me)
        {
            return JsonSerializer.Serialize(me, new JsonSerializerOptions { WriteIndented = true, ReferenceHandler = ReferenceHandler.Preserve });
        }

        public static void Write<T>(this RepositoryResult<T> me)
        {
            File.WriteAllText($"C:\\Temp\\{me.Item}.json", me.Json());
        }
    }
}