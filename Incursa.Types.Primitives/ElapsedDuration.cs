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

/// <summary>Represents a signed exact count of 100-nanosecond elapsed units.</summary>
[JsonConverter(typeof(ElapsedDurationJsonConverter))]
public readonly record struct ElapsedDuration
    : IComparable,
      IComparable<ElapsedDuration>,
      ISpanFormattable,
      ISpanParsable<ElapsedDuration>
{
    /// <summary>Number of exact units in one second.</summary>
    public const long UnitsPerSecond = TimeSpan.TicksPerSecond;

    /// <summary>Represents no elapsed time.</summary>
    public static readonly ElapsedDuration Zero = default;

    /// <summary>Initializes a duration from an exact signed unit count.</summary>
    public ElapsedDuration(long units)
    {
        Units = units;
    }

    /// <summary>Gets the signed 100-nanosecond unit count.</summary>
    public long Units { get; }

    /// <summary>Creates a duration from an exact signed unit count.</summary>
    public static ElapsedDuration FromUnits(long units) => new(units);

    /// <summary>Creates a duration from a TimeSpan without precision loss.</summary>
    public static ElapsedDuration FromTimeSpanExact(TimeSpan value) => new(value.Ticks);

    /// <summary>Returns the exactly equivalent TimeSpan.</summary>
    public TimeSpan ToTimeSpanExact() => TimeSpan.FromTicks(Units);

    /// <summary>Returns the checked exact sum.</summary>
    public ElapsedDuration Add(ElapsedDuration other) => new(checked(Units + other.Units));

    /// <summary>Returns the checked exact difference.</summary>
    public ElapsedDuration Subtract(ElapsedDuration other) => new(checked(Units - other.Units));

    /// <summary>Parses the exact elapsed-duration lexical profile.</summary>
    public static ElapsedDuration Parse(string s)
    {
        ArgumentNullException.ThrowIfNull(s);
        return Parse(s.AsSpan(), CultureInfo.InvariantCulture);
    }

    /// <inheritdoc/>
    public static ElapsedDuration Parse(string s, IFormatProvider? provider)
    {
        ArgumentNullException.ThrowIfNull(s);
        return Parse(s.AsSpan(), provider);
    }

    /// <inheritdoc/>
    public static ElapsedDuration Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        if (!TryParseCore(s, out ElapsedDuration result))
        {
            throw new FormatException("The value is not a valid exact ElapsedDuration.");
        }

        return result;
    }

    /// <summary>Attempts to parse the exact elapsed-duration lexical profile.</summary>
    public static bool TryParse([NotNullWhen(true)] string? s, out ElapsedDuration result) =>
        TryParse(s, CultureInfo.InvariantCulture, out result);

    /// <inheritdoc/>
    public static bool TryParse(
        [NotNullWhen(true)] string? s,
        IFormatProvider? provider,
        out ElapsedDuration result)
    {
        if (s is null)
        {
            result = default;
            return false;
        }

        return TryParseCore(s.AsSpan(), out result);
    }

    /// <inheritdoc/>
    public static bool TryParse(
        ReadOnlySpan<char> s,
        IFormatProvider? provider,
        out ElapsedDuration result) => TryParseCore(s, out result);

    /// <inheritdoc/>
    public int CompareTo(object? obj)
    {
        if (obj is null)
        {
            return 1;
        }

        return obj is ElapsedDuration other
            ? CompareTo(other)
            : throw new ArgumentException($"Object must be of type {nameof(ElapsedDuration)}.", nameof(obj));
    }

    /// <inheritdoc/>
    public int CompareTo(ElapsedDuration other) => Units.CompareTo(other.Units);

    /// <inheritdoc/>
    public override string ToString()
    {
        if (Units == 0)
        {
            return "PT0S";
        }

        bool negative = Units < 0;
        ulong magnitude = negative
            ? (ulong)(-(Units + 1)) + 1
            : (ulong)Units;
        ulong seconds = magnitude / (ulong)UnitsPerSecond;
        uint fraction = (uint)(magnitude % (ulong)UnitsPerSecond);
        string prefix = negative ? "-PT" : "PT";
        if (fraction == 0)
        {
            return string.Create(CultureInfo.InvariantCulture, $"{prefix}{seconds}S");
        }

        string fractionalText = fraction.ToString("D7", CultureInfo.InvariantCulture).TrimEnd('0');
        return string.Create(
            CultureInfo.InvariantCulture,
            $"{prefix}{seconds}.{fractionalText}S");
    }

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

        string text = ToString();
        if (!text.AsSpan().TryCopyTo(destination))
        {
            charsWritten = 0;
            return false;
        }

        charsWritten = text.Length;
        return true;
    }

    public static ElapsedDuration operator +(ElapsedDuration left, ElapsedDuration right) => left.Add(right);

    public static ElapsedDuration operator -(ElapsedDuration left, ElapsedDuration right) => left.Subtract(right);

    public static ElapsedDuration operator +(ElapsedDuration value) => value;

    public static ElapsedDuration operator -(ElapsedDuration value) => new(checked(-value.Units));

    public static bool operator <(ElapsedDuration left, ElapsedDuration right) => left.Units < right.Units;

    public static bool operator <=(ElapsedDuration left, ElapsedDuration right) => left.Units <= right.Units;

    public static bool operator >(ElapsedDuration left, ElapsedDuration right) => left.Units > right.Units;

    public static bool operator >=(ElapsedDuration left, ElapsedDuration right) => left.Units >= right.Units;

    private static bool TryParseCore(ReadOnlySpan<char> text, out ElapsedDuration result)
    {
        result = default;
        if (text.Length < 4)
        {
            return false;
        }

        int index = 0;
        bool negative = false;
        if (text[index] == '-')
        {
            negative = true;
            index++;
        }

        if (index + 2 > text.Length || text[index] != 'P' || text[index + 1] != 'T')
        {
            return false;
        }

        index += 2;
        int integerStart = index;
        ulong seconds = 0;
        while (index < text.Length && text[index] is >= '0' and <= '9')
        {
            uint digit = (uint)(text[index] - '0');
            if (seconds > (ulong.MaxValue - digit) / 10)
            {
                return false;
            }

            seconds = (seconds * 10) + digit;
            index++;
        }

        if (index == integerStart)
        {
            return false;
        }

        uint fraction = 0;
        int fractionalDigits = 0;
        if (index < text.Length && text[index] == '.')
        {
            index++;
            int fractionStart = index;
            while (index < text.Length && text[index] is >= '0' and <= '9')
            {
                if (fractionalDigits == 7)
                {
                    return false;
                }

                fraction = (fraction * 10) + (uint)(text[index] - '0');
                fractionalDigits++;
                index++;
            }

            if (index == fractionStart)
            {
                return false;
            }

            for (int i = fractionalDigits; i < 7; i++)
            {
                fraction *= 10;
            }
        }

        if (index != text.Length - 1 || text[index] != 'S')
        {
            return false;
        }

        ulong maximumMagnitude = negative ? (ulong)long.MaxValue + 1 : long.MaxValue;
        if (seconds > maximumMagnitude / (ulong)UnitsPerSecond)
        {
            return false;
        }

        ulong units = seconds * (ulong)UnitsPerSecond;
        if (units > maximumMagnitude - fraction)
        {
            return false;
        }

        units += fraction;
        long signedUnits = negative
            ? units == (ulong)long.MaxValue + 1 ? long.MinValue : -(long)units
            : (long)units;
        result = new ElapsedDuration(signedUnits);
        return true;
    }
}

/// <summary>Serializes ElapsedDuration as canonical exact text.</summary>
public sealed class ElapsedDurationJsonConverter : JsonConverter<ElapsedDuration>
{
    /// <inheritdoc/>
    public override ElapsedDuration Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) =>
        ElapsedDuration.TryParse(reader.GetString(), out ElapsedDuration result)
            ? result
            : throw new JsonException("Invalid ElapsedDuration text.");

    /// <inheritdoc/>
    public override void Write(
        Utf8JsonWriter writer,
        ElapsedDuration value,
        JsonSerializerOptions options) => writer.WriteStringValue(value.ToString());
}
