// Copyright (c) Samuel McAravey
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.

using System.Globalization;

namespace Incursa;

/// <summary>Represents an exact half-open interval whose endpoint offsets are part of its value.</summary>
[JsonConverter(typeof(ExactPeriodJsonConverter))]
public readonly struct ExactPeriod : IEquatable<ExactPeriod>
{
    private const string EndpointFormat = "yyyy-MM-dd'T'HH:mm:ss.fffffffzzz";

    /// <summary>Initializes an exact half-open interval.</summary>
    public ExactPeriod(DateTimeOffset startInclusive, DateTimeOffset endExclusive)
    {
        if (endExclusive.UtcTicks < startInclusive.UtcTicks)
        {
            throw new ArgumentException("The end instant cannot precede the start instant.", nameof(endExclusive));
        }

        StartInclusive = startInclusive;
        EndExclusive = endExclusive;
    }

    /// <summary>Gets the inclusive endpoint.</summary>
    public DateTimeOffset StartInclusive { get; }

    /// <summary>Gets the exclusive endpoint.</summary>
    public DateTimeOffset EndExclusive { get; }

    /// <summary>Gets whether this interval contains no instants.</summary>
    public bool IsEmpty => StartInclusive.UtcTicks == EndExclusive.UtcTicks;

    /// <summary>Gets the exact elapsed duration between endpoint instants.</summary>
    public ElapsedDuration Elapsed => new(checked(EndExclusive.UtcTicks - StartInclusive.UtcTicks));

    /// <summary>Creates an interval from a start endpoint and exact elapsed duration.</summary>
    public static ExactPeriod FromElapsed(DateTimeOffset startInclusive, ElapsedDuration elapsed)
    {
        if (elapsed.Units < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(elapsed), "Elapsed duration cannot be negative.");
        }

        return new ExactPeriod(startInclusive, startInclusive.AddTicks(elapsed.Units));
    }

    /// <summary>Creates an interval by applying a calendar duration with explicit zone resolution.</summary>
    public static ExactPeriod FromCalendarDuration(
        LocalDateTime start,
        CalendarDuration duration,
        TimeZoneInfo timeZone,
        LocalTimeResolution resolution)
    {
        DateTimeOffset resolvedStart = start.InTimeZone(timeZone, resolution);
        DateTimeOffset resolvedEnd = duration.ApplyInTimeZone(start, timeZone, resolution);
        return new ExactPeriod(resolvedStart, resolvedEnd);
    }

    /// <summary>Returns whether the instant lies in this half-open interval.</summary>
    public bool Contains(DateTimeOffset instant) =>
        instant.UtcTicks >= StartInclusive.UtcTicks && instant.UtcTicks < EndExclusive.UtcTicks;

    /// <summary>Returns whether this nonempty interval intersects another nonempty interval.</summary>
    public bool Overlaps(ExactPeriod other) =>
        !IsEmpty && !other.IsEmpty &&
        StartInclusive.UtcTicks < other.EndExclusive.UtcTicks &&
        other.StartInclusive.UtcTicks < EndExclusive.UtcTicks;

    /// <summary>Returns whether both endpoint instants match, ignoring their declared offsets.</summary>
    public bool IsInstantEquivalentTo(ExactPeriod other) =>
        StartInclusive.UtcTicks == other.StartInclusive.UtcTicks &&
        EndExclusive.UtcTicks == other.EndExclusive.UtcTicks;

    /// <summary>Parses two canonical offset endpoints separated by a slash.</summary>
    public static ExactPeriod Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return TryParse(value, out ExactPeriod result)
            ? result
            : throw new FormatException("The value is not a valid ExactPeriod.");
    }

    /// <summary>Attempts to parse two canonical offset endpoints separated by a slash.</summary>
    public static bool TryParse([NotNullWhen(true)] string? value, out ExactPeriod result)
    {
        result = default;
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        int separator = value.IndexOf('/');
        if (separator <= 0 || separator != value.LastIndexOf('/') || separator == value.Length - 1 ||
            !DateTimeOffset.TryParseExact(
                value.AsSpan(0, separator),
                EndpointFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTimeOffset start) ||
            !DateTimeOffset.TryParseExact(
                value.AsSpan(separator + 1),
                EndpointFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTimeOffset end) ||
            end.UtcTicks < start.UtcTicks)
        {
            return false;
        }

        result = new ExactPeriod(start, end);
        return true;
    }

    /// <inheritdoc/>
    public bool Equals(ExactPeriod other) =>
        StartInclusive.Ticks == other.StartInclusive.Ticks &&
        StartInclusive.Offset == other.StartInclusive.Offset &&
        EndExclusive.Ticks == other.EndExclusive.Ticks &&
        EndExclusive.Offset == other.EndExclusive.Offset;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ExactPeriod other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(
        StartInclusive.Ticks,
        StartInclusive.Offset,
        EndExclusive.Ticks,
        EndExclusive.Offset);

    /// <inheritdoc/>
    public override string ToString() => string.Create(
        CultureInfo.InvariantCulture,
        $"{StartInclusive.ToString(EndpointFormat, CultureInfo.InvariantCulture)}/{EndExclusive.ToString(EndpointFormat, CultureInfo.InvariantCulture)}");

    public static bool operator ==(ExactPeriod left, ExactPeriod right) => left.Equals(right);

    public static bool operator !=(ExactPeriod left, ExactPeriod right) => !left.Equals(right);
}

/// <summary>Serializes ExactPeriod using two canonical offset endpoints.</summary>
public sealed class ExactPeriodJsonConverter : JsonConverter<ExactPeriod>
{
    /// <inheritdoc/>
    public override ExactPeriod Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) =>
        ExactPeriod.TryParse(reader.GetString(), out ExactPeriod value)
            ? value
            : throw new JsonException("Invalid ExactPeriod text.");

    /// <inheritdoc/>
    public override void Write(
        Utf8JsonWriter writer,
        ExactPeriod value,
        JsonSerializerOptions options) => writer.WriteStringValue(value.ToString());
}
