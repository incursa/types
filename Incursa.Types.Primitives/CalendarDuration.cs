// Copyright (c) Samuel McAravey
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.

using System.Globalization;
using System.Numerics;
using System.Text;

namespace Incursa;

/// <summary>
/// Represents a strict calendar duration consisting of integral months, integral days,
/// and exact 100-nanosecond elapsed units.
/// </summary>
[JsonConverter(typeof(CalendarDurationJsonConverter))]
public readonly record struct CalendarDuration
{
    /// <summary>Represents a zero calendar duration.</summary>
    public static readonly CalendarDuration Zero = default;

    /// <summary>Initializes a strict calendar duration.</summary>
    public CalendarDuration(int months, int days, ElapsedDuration elapsed)
    {
        int sign = Math.Sign(months);
        sign = MergeSign(sign, Math.Sign(days));
        _ = MergeSign(sign, Math.Sign(elapsed.Units));
        Months = months;
        Days = days;
        Elapsed = elapsed;
    }

    /// <summary>Gets the signed integral month component.</summary>
    public int Months { get; }

    /// <summary>Gets the signed integral day component.</summary>
    public int Days { get; }

    /// <summary>Gets the signed exact elapsed component.</summary>
    public ElapsedDuration Elapsed { get; }

    /// <summary>Applies months, then days, then elapsed units to zone-free local fields.</summary>
    public LocalDateTime Apply(LocalDateTime start)
    {
        DateTime value = start.Value.AddMonths(Months).AddDays(Days).AddTicks(Elapsed.Units);
        return new LocalDateTime(value);
    }

    /// <summary>
    /// Applies calendar components in local time, resolves them explicitly in the named zone,
    /// then applies the exact elapsed component on the instant timeline.
    /// </summary>
    public DateTimeOffset ApplyInTimeZone(
        LocalDateTime start,
        TimeZoneInfo timeZone,
        LocalTimeResolution resolution)
    {
        DateTime calendarValue = start.Value.AddMonths(Months).AddDays(Days);
        DateTimeOffset resolved = new LocalDateTime(calendarValue).InTimeZone(timeZone, resolution);
        DateTimeOffset exactInstant = resolved.AddTicks(Elapsed.Units);
        return TimeZoneInfo.ConvertTime(exactInstant, timeZone);
    }

    /// <summary>Parses the strict calendar-duration lexical profile.</summary>
    public static CalendarDuration Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return TryParse(value, out CalendarDuration result)
            ? result
            : throw new FormatException("The value is not a valid strict CalendarDuration.");
    }

    /// <summary>Attempts to parse the strict calendar-duration lexical profile.</summary>
    public static bool TryParse([NotNullWhen(true)] string? value, out CalendarDuration result)
    {
        result = default;
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        ReadOnlySpan<char> text = value.AsSpan();
        int index = 0;
        int sign = 1;
        if (text[index] == '-')
        {
            sign = -1;
            index++;
        }

        if (index >= text.Length || text[index++] != 'P')
        {
            return false;
        }

        BigInteger months = BigInteger.Zero;
        BigInteger days = BigInteger.Zero;
        BigInteger elapsed = BigInteger.Zero;
        bool any = false;
        int dateOrder = 0;
        while (index < text.Length && text[index] != 'T')
        {
            if (!TryReadNumber(text, ref index, allowFraction: false, out BigInteger number, out int scale) ||
                scale != 0 || index >= text.Length)
            {
                return false;
            }

            char designator = text[index++];
            int order = designator switch { 'Y' => 1, 'M' => 2, 'W' => 3, 'D' => 4, _ => 0 };
            if (order == 0 || order <= dateOrder)
            {
                return false;
            }

            dateOrder = order;
            any = true;
            if (designator == 'Y')
            {
                months += number * 12;
            }
            else if (designator == 'M')
            {
                months += number;
            }
            else if (designator == 'W')
            {
                days += number * 7;
            }
            else
            {
                days += number;
            }
        }

        if (index < text.Length && text[index] == 'T')
        {
            index++;
            bool anyTime = false;
            int timeOrder = 0;
            while (index < text.Length)
            {
                if (!TryReadNumber(text, ref index, allowFraction: true, out BigInteger number, out int scale) ||
                    index >= text.Length)
                {
                    return false;
                }

                char designator = text[index++];
                int order = designator switch { 'H' => 1, 'M' => 2, 'S' => 3, _ => 0 };
                if (order == 0 || order <= timeOrder)
                {
                    return false;
                }

                BigInteger unitsPerWhole = designator switch
                {
                    'H' => TimeSpan.TicksPerHour,
                    'M' => TimeSpan.TicksPerMinute,
                    _ => TimeSpan.TicksPerSecond,
                };
                BigInteger divisor = BigInteger.Pow(10, scale);
                BigInteger scaled = number * unitsPerWhole;
                if (scaled % divisor != 0)
                {
                    return false;
                }

                elapsed += scaled / divisor;
                timeOrder = order;
                anyTime = true;
                any = true;
            }

            if (!anyTime)
            {
                return false;
            }
        }

        months *= sign;
        days *= sign;
        elapsed *= sign;
        if (!any || months < int.MinValue || months > int.MaxValue ||
            days < int.MinValue || days > int.MaxValue ||
            elapsed < long.MinValue || elapsed > long.MaxValue)
        {
            return false;
        }

        result = new CalendarDuration((int)months, (int)days, new ElapsedDuration((long)elapsed));
        return true;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        if (this == Zero)
        {
            return "P0D";
        }

        bool negative = Months < 0 || Days < 0 || Elapsed.Units < 0;
        BigInteger months = BigInteger.Abs(new BigInteger(Months));
        BigInteger days = BigInteger.Abs(new BigInteger(Days));
        BigInteger units = BigInteger.Abs(new BigInteger(Elapsed.Units));
        var builder = new StringBuilder();
        if (negative)
        {
            builder.Append('-');
        }

        builder.Append('P');
        if (!months.IsZero)
        {
            builder.Append(months.ToString(CultureInfo.InvariantCulture)).Append('M');
        }

        if (!days.IsZero)
        {
            builder.Append(days.ToString(CultureInfo.InvariantCulture)).Append('D');
        }

        if (!units.IsZero)
        {
            BigInteger seconds = BigInteger.DivRem(units, TimeSpan.TicksPerSecond, out BigInteger fraction);
            builder.Append('T').Append(seconds.ToString(CultureInfo.InvariantCulture));
            if (!fraction.IsZero)
            {
                builder.Append('.').Append(
                    ((int)fraction).ToString("D7", CultureInfo.InvariantCulture).TrimEnd('0'));
            }

            builder.Append('S');
        }

        return builder.ToString();
    }

    private static int MergeSign(int current, int next)
    {
        if (current != 0 && next != 0 && current != next)
        {
            throw new ArgumentException(
                "All nonzero CalendarDuration components must have the same sign.",
                nameof(next));
        }

        return current == 0 ? next : current;
    }

    private static bool TryReadNumber(
        ReadOnlySpan<char> text,
        ref int index,
        bool allowFraction,
        out BigInteger number,
        out int scale)
    {
        number = BigInteger.Zero;
        scale = 0;
        int start = index;
        while (index < text.Length && text[index] is >= '0' and <= '9')
        {
            number = (number * 10) + (text[index++] - '0');
        }

        if (index == start)
        {
            return false;
        }

        if (index < text.Length && text[index] == '.')
        {
            if (!allowFraction)
            {
                return false;
            }

            index++;
            int fractionStart = index;
            while (index < text.Length && text[index] is >= '0' and <= '9')
            {
                number = (number * 10) + (text[index++] - '0');
                scale++;
            }

            if (index == fractionStart)
            {
                return false;
            }
        }

        return true;
    }
}

/// <summary>Serializes CalendarDuration using its strict canonical profile.</summary>
public sealed class CalendarDurationJsonConverter : JsonConverter<CalendarDuration>
{
    /// <inheritdoc/>
    public override CalendarDuration Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) =>
        CalendarDuration.TryParse(reader.GetString(), out CalendarDuration value)
            ? value
            : throw new JsonException("Invalid CalendarDuration text.");

    /// <inheritdoc/>
    public override void Write(
        Utf8JsonWriter writer,
        CalendarDuration value,
        JsonSerializerOptions options) => writer.WriteStringValue(value.ToString());
}
