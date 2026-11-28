// Copyright (c) Samuel McAravey
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Globalization;

namespace Incursa;

/// <summary>Represents Gregorian local calendar fields without a zone or offset.</summary>
[JsonConverter(typeof(LocalDateTimeJsonConverter))]
public readonly record struct LocalDateTime
    : IComparable,
      IComparable<LocalDateTime>,
      ISpanFormattable,
      ISpanParsable<LocalDateTime>
{
    private const string CanonicalFormat = "yyyy-MM-dd'T'HH:mm:ss.fffffff";

    /// <summary>Gets the first value in the base Gregorian profile.</summary>
    public static LocalDateTime MinValue { get; } = new(DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Unspecified));

    /// <summary>Gets the last value in the base Gregorian profile.</summary>
    public static LocalDateTime MaxValue { get; } = new(DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Unspecified));

    /// <summary>Initializes a value from explicit Gregorian fields.</summary>
    public LocalDateTime(
        int year,
        int month,
        int day,
        int hour,
        int minute,
        int second,
        int subsecondTicks = 0)
        : this(new DateTime(year, month, day, hour, minute, second, DateTimeKind.Unspecified)
            .AddTicks(ValidateSubsecondTicks(subsecondTicks)))
    {
    }

    /// <summary>Initializes a value from an explicitly unspecified DateTime.</summary>
    public LocalDateTime(DateTime value)
    {
        if (value.Kind != DateTimeKind.Unspecified)
        {
            throw new ArgumentException(
                "LocalDateTime requires DateTimeKind.Unspecified; remove zone semantics explicitly first.",
                nameof(value));
        }

        Value = value;
    }

    /// <summary>Gets the underlying Gregorian fields as an unspecified DateTime.</summary>
    public DateTime Value { get; }

    /// <summary>Gets ticks since 0001-01-01 without timezone interpretation.</summary>
    public long Ticks => Value.Ticks;

    /// <summary>Creates local Gregorian fields from exact ticks since 0001-01-01.</summary>
    public static LocalDateTime FromTicks(long ticks) =>
        new(new DateTime(ticks, DateTimeKind.Unspecified));

    public int Year => Value.Year;

    public int Month => Value.Month;

    public int Day => Value.Day;

    public int Hour => Value.Hour;

    public int Minute => Value.Minute;

    public int Second => Value.Second;

    public int SubsecondTicks => (int)(Value.Ticks % TimeSpan.TicksPerSecond);

    /// <summary>Returns the checked local-field result after exact elapsed addition.</summary>
    public LocalDateTime Add(ElapsedDuration duration)
    {
        try
        {
            return new LocalDateTime(Value.AddTicks(duration.Units));
        }
        catch (ArgumentOutOfRangeException exception)
        {
            throw new OverflowException("The LocalDateTime result is outside years 0001 through 9999.", exception);
        }
    }

    /// <summary>Returns the checked local-field result after exact elapsed subtraction.</summary>
    public LocalDateTime Subtract(ElapsedDuration duration)
    {
        try
        {
            return new LocalDateTime(Value.AddTicks(checked(-duration.Units)));
        }
        catch (Exception exception) when (exception is ArgumentOutOfRangeException or OverflowException)
        {
            throw new OverflowException("The LocalDateTime result is outside years 0001 through 9999.", exception);
        }
    }

    /// <summary>Returns the exact elapsed field difference.</summary>
    public ElapsedDuration Subtract(LocalDateTime other) => new(checked(Ticks - other.Ticks));

    /// <summary>Resolves these fields with an explicit UTC offset.</summary>
    public DateTimeOffset WithOffset(TimeSpan offset) => new(Value, offset);

    /// <summary>
    /// Resolves these fields in a named zone, rejecting ambiguous and invalid
    /// local times rather than choosing an offset implicitly.
    /// </summary>
    public DateTimeOffset InTimeZone(TimeZoneInfo timeZone)
    {
        ArgumentNullException.ThrowIfNull(timeZone);
        if (timeZone.IsInvalidTime(Value))
        {
            throw new ArgumentException(
                $"Local time {this} does not exist in time zone '{timeZone.Id}'.",
                nameof(timeZone));
        }

        if (timeZone.IsAmbiguousTime(Value))
        {
            throw new ArgumentException(
                $"Local time {this} is ambiguous in time zone '{timeZone.Id}'.",
                nameof(timeZone));
        }

        return new DateTimeOffset(Value, timeZone.GetUtcOffset(Value));
    }

    /// <summary>
    /// Resolves these fields using an explicit ambiguity or gap policy.
    /// </summary>
    public DateTimeOffset InTimeZone(
        TimeZoneInfo timeZone,
        LocalTimeResolution resolution)
    {
        ArgumentNullException.ThrowIfNull(timeZone);
        if (!Enum.IsDefined(resolution))
        {
            throw new ArgumentOutOfRangeException(nameof(resolution));
        }

        if (timeZone.IsAmbiguousTime(Value))
        {
            TimeSpan[] offsets = timeZone.GetAmbiguousTimeOffsets(Value);
            TimeSpan offset = resolution switch
            {
                LocalTimeResolution.EarlierInstant => offsets.Max(),
                LocalTimeResolution.LaterInstant => offsets.Min(),
                LocalTimeResolution.Reject => throw CreateAmbiguousException(timeZone),
                _ => throw new ArgumentException(
                    "Ambiguous local times require EarlierInstant or LaterInstant resolution.",
                    nameof(resolution)),
            };
            return new DateTimeOffset(Value, offset);
        }

        if (timeZone.IsInvalidTime(Value))
        {
            if (resolution == LocalTimeResolution.Reject)
            {
                throw CreateInvalidException(timeZone);
            }

            if (resolution is not LocalTimeResolution.ShiftForward and not LocalTimeResolution.ShiftBackward)
            {
                throw new ArgumentException(
                    "Nonexistent local times require ShiftForward or ShiftBackward resolution.",
                    nameof(resolution));
            }

            TimeSpan gap = FindInvalidTimeGap(timeZone);
            DateTime shifted = resolution == LocalTimeResolution.ShiftForward
                ? Value.Add(gap)
                : Value.Subtract(gap);
            return new LocalDateTime(shifted).InTimeZone(timeZone);
        }

        return new DateTimeOffset(Value, timeZone.GetUtcOffset(Value));
    }

    /// <summary>Parses canonical local date-time text.</summary>
    public static LocalDateTime Parse(string s)
    {
        ArgumentNullException.ThrowIfNull(s);
        return Parse(s.AsSpan(), CultureInfo.InvariantCulture);
    }

    /// <inheritdoc/>
    public static LocalDateTime Parse(string s, IFormatProvider? provider)
    {
        ArgumentNullException.ThrowIfNull(s);
        return Parse(s.AsSpan(), provider);
    }

    /// <inheritdoc/>
    public static LocalDateTime Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        if (!TryParse(s, provider, out LocalDateTime result))
        {
            throw new FormatException("The value is not canonical LocalDateTime text.");
        }

        return result;
    }

    /// <summary>Attempts to parse canonical local date-time text.</summary>
    public static bool TryParse([NotNullWhen(true)] string? s, out LocalDateTime result) =>
        TryParse(s, CultureInfo.InvariantCulture, out result);

    /// <inheritdoc/>
    public static bool TryParse(
        [NotNullWhen(true)] string? s,
        IFormatProvider? provider,
        out LocalDateTime result)
    {
        if (s is null)
        {
            result = default;
            return false;
        }

        return TryParse(s.AsSpan(), provider, out result);
    }

    /// <inheritdoc/>
    public static bool TryParse(
        ReadOnlySpan<char> s,
        IFormatProvider? provider,
        out LocalDateTime result)
    {
        if (s.Length == 27 &&
            DateTime.TryParseExact(
                s,
                CanonicalFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime value) &&
            value.Kind == DateTimeKind.Unspecified)
        {
            result = new LocalDateTime(value);
            return true;
        }

        result = default;
        return false;
    }

    /// <inheritdoc/>
    public int CompareTo(object? obj)
    {
        if (obj is null)
        {
            return 1;
        }

        return obj is LocalDateTime other
            ? CompareTo(other)
            : throw new ArgumentException($"Object must be of type {nameof(LocalDateTime)}.", nameof(obj));
    }

    /// <inheritdoc/>
    public int CompareTo(LocalDateTime other) => Ticks.CompareTo(other.Ticks);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString(CanonicalFormat, CultureInfo.InvariantCulture);

    /// <inheritdoc/>
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        if (!string.IsNullOrEmpty(format) && !string.Equals(format, "G", StringComparison.OrdinalIgnoreCase))
        {
            throw new FormatException($"The '{format}' format is not supported.");
        }

        return ToString();
    }

    /// <inheritdoc/>
    public bool TryFormat(
        Span<char> destination,
        out int charsWritten,
        ReadOnlySpan<char> format,
        IFormatProvider? provider)
    {
        if (!format.IsEmpty && !format.Equals("G", StringComparison.OrdinalIgnoreCase))
        {
            charsWritten = 0;
            return false;
        }

        return Value.TryFormat(destination, out charsWritten, CanonicalFormat, CultureInfo.InvariantCulture);
    }

    public static LocalDateTime operator +(LocalDateTime value, ElapsedDuration duration) => value.Add(duration);

    public static LocalDateTime operator -(LocalDateTime value, ElapsedDuration duration) => value.Subtract(duration);

    public static ElapsedDuration operator -(LocalDateTime left, LocalDateTime right) => left.Subtract(right);

    public static bool operator <(LocalDateTime left, LocalDateTime right) => left.Ticks < right.Ticks;

    public static bool operator <=(LocalDateTime left, LocalDateTime right) => left.Ticks <= right.Ticks;

    public static bool operator >(LocalDateTime left, LocalDateTime right) => left.Ticks > right.Ticks;

    public static bool operator >=(LocalDateTime left, LocalDateTime right) => left.Ticks >= right.Ticks;

    private static int ValidateSubsecondTicks(int subsecondTicks)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(subsecondTicks);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(subsecondTicks, TimeSpan.TicksPerSecond);
        return subsecondTicks;
    }

    private TimeSpan FindInvalidTimeGap(TimeZoneInfo timeZone)
    {
        DateTime before = Value;
        DateTime after = Value;
        for (int minutes = 0; minutes < 24 * 60; minutes++)
        {
            before = before.AddMinutes(-1);
            after = after.AddMinutes(1);
            if (!timeZone.IsInvalidTime(before) && !timeZone.IsInvalidTime(after))
            {
                TimeSpan gap = timeZone.GetUtcOffset(after) - timeZone.GetUtcOffset(before);
                if (gap > TimeSpan.Zero)
                {
                    return gap;
                }

                break;
            }
        }

        throw new InvalidTimeZoneException(
            $"Unable to determine the transition gap for local time {this} in '{timeZone.Id}'.");
    }

    private ArgumentException CreateAmbiguousException(TimeZoneInfo timeZone) =>
        new($"Local time {this} is ambiguous in time zone '{timeZone.Id}'.", nameof(timeZone));

    private ArgumentException CreateInvalidException(TimeZoneInfo timeZone) =>
        new($"Local time {this} does not exist in time zone '{timeZone.Id}'.", nameof(timeZone));
}

/// <summary>Serializes LocalDateTime as canonical zone-free text.</summary>
public sealed class LocalDateTimeJsonConverter : JsonConverter<LocalDateTime>
{
    /// <inheritdoc/>
    public override LocalDateTime Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) =>
        LocalDateTime.TryParse(reader.GetString(), out LocalDateTime result)
            ? result
            : throw new JsonException("Invalid LocalDateTime text.");

    /// <inheritdoc/>
    public override void Write(
        Utf8JsonWriter writer,
        LocalDateTime value,
        JsonSerializerOptions options) => writer.WriteStringValue(value.ToString());
}
