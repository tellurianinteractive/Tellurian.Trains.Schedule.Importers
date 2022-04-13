using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Tellurian.Trains.Models.Planning.Resources;

namespace Tellurian.Trains
{
    public struct Maybe<T> : IEquatable<Maybe<T>> where T : class
    {
        //public static Maybe<T> ItemIfOne(IEnumerable<T> values, )

        public static Maybe<T> None => new(Strings.NoValue);

        public Maybe(T? value) { _Value = value; Message = string.Empty; }
        public Maybe(string message) { _Value = null; Message = message; }
        public Maybe(IEnumerable<string> messages) { _Value = null; Message = string.Join(", ", messages); }
        public Maybe(string format, params object[] args) : this(string.Format(CultureInfo.InvariantCulture, format, args)) { }
        public Maybe(T? value, string message) { _Value = value; Message = message; }
        public Maybe(T? value, string format, params object[] args) : this(value, string.Format(CultureInfo.InvariantCulture, format, args)) { }
        public Maybe(IEnumerable<T>? values, string message) { _Value = values?.FirstOrDefault(); Message = _Value is null ? message : string.Empty; }

        private readonly T? _Value;
        public T Value => _Value ?? throw new InvalidOperationException(Strings.NoValue);
        public bool HasValue => _Value != null;
        public bool IsNone => !HasValue;
        public string Message { get; }

        public bool Equals(Maybe<T> other) => _Value?.Equals(other._Value) ?? false;
        public override bool Equals(object? obj) => (obj is Maybe<T> other && Equals(other)) || (obj is T instance && instance.Equals(_Value));
        public override int GetHashCode() => _Value?.GetHashCode() ?? Message.GetHashCode(StringComparison.OrdinalIgnoreCase);

        public static bool operator ==(Maybe<T> left, Maybe<T> right) => left.Equals(right);
        public static bool operator !=(Maybe<T> left, Maybe<T> right) => !(left == right);

        public override string ToString() =>
            HasValue ? Value.ToString() ?? string.Empty : Message;
    }
}
