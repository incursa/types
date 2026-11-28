// Copyright (c) Samuel McAravey
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.

using System.Text.Json;

namespace Incursa.Types.Tests;

public sealed class StrictCalendarTemporalTests
{
    [Theory]
    [InlineData("P1Y2M3W4D", 14, 25, 0, "P14M25D")]
    [InlineData("PT1.5H", 0, 0, 54000000000, "PT5400S")]
    [InlineData("PT1.25M", 0, 0, 750000000, "PT75S")]
    [InlineData("PT1.0000001S", 0, 0, 10000001, "PT1.0000001S")]
    [InlineData("-P1M2DT3.5S", -1, -2, -35000000, "-P1M2DT3.5S")]
    [InlineData("P0Y0M0W0DT0S", 0, 0, 0, "P0D")]
    public void CalendarDurationNormalizesStrictInput(
        string input,
        int months,
        int days,
        long units,
        string canonical)
    {
        CalendarDuration value = CalendarDuration.Parse(input);

        value.Months.ShouldBe(months);
        value.Days.ShouldBe(days);
        value.Elapsed.Units.ShouldBe(units);
        value.ToString().ShouldBe(canonical);
        CalendarDuration.Parse(canonical).ShouldBe(value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("P")]
    [InlineData("PT")]
    [InlineData("+P1D")]
    [InlineData("P1.5M")]
    [InlineData("P1.5D")]
    [InlineData("P1D-PT1S")]
    [InlineData("P-1D")]
    [InlineData("P1D1M")]
    [InlineData("PT1S1H")]
    [InlineData("PT0.00000001S")]
    [InlineData("p1d")]
    public void CalendarDurationRejectsNonStrictOrInexactInput(string input)
    {
        CalendarDuration.TryParse(input, out _).ShouldBeFalse();
        Should.Throw<FormatException>(() => CalendarDuration.Parse(input));
    }

    [Fact]
    public void CalendarDurationRequiresOneSignAndAppliesComponentsInOrder()
    {
        Should.Throw<ArgumentException>(
            () => new CalendarDuration(1, -1, ElapsedDuration.Zero));

        LocalDateTime start = LocalDateTime.Parse("2024-01-31T23:59:59.9999999");
        var duration = new CalendarDuration(1, 1, new ElapsedDuration(1));

        duration.Apply(start).ToString().ShouldBe("2024-03-02T00:00:00.0000000");
        new CalendarDuration(-1, -1, new ElapsedDuration(-1))
            .Apply(LocalDateTime.Parse("2024-03-31T00:00:00.0000001"))
            .ToString()
            .ShouldBe("2024-02-28T00:00:00.0000000");
    }

    [Fact]
    public void LocalTimeResolutionHandlesAmbiguitiesAndGapsExplicitly()
    {
        TimeZoneInfo zone = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
        LocalDateTime ambiguous = LocalDateTime.Parse("2024-11-03T01:30:00.0000000");
        LocalDateTime invalid = LocalDateTime.Parse("2024-03-10T02:30:00.0000000");

        DateTimeOffset earlier = ambiguous.InTimeZone(zone, LocalTimeResolution.EarlierInstant);
        DateTimeOffset later = ambiguous.InTimeZone(zone, LocalTimeResolution.LaterInstant);
        earlier.Offset.ShouldBe(TimeSpan.FromHours(-4));
        later.Offset.ShouldBe(TimeSpan.FromHours(-5));
        (later.UtcTicks - earlier.UtcTicks).ShouldBe(TimeSpan.TicksPerHour);
        Should.Throw<ArgumentException>(
            () => ambiguous.InTimeZone(zone, LocalTimeResolution.Reject));
        Should.Throw<ArgumentException>(
            () => ambiguous.InTimeZone(zone, LocalTimeResolution.ShiftForward));

        invalid.InTimeZone(zone, LocalTimeResolution.ShiftForward)
            .ToString("yyyy-MM-dd'T'HH:mm:ss.fffffffzzz")
            .ShouldBe("2024-03-10T03:30:00.0000000-04:00");
        invalid.InTimeZone(zone, LocalTimeResolution.ShiftBackward)
            .ToString("yyyy-MM-dd'T'HH:mm:ss.fffffffzzz")
            .ShouldBe("2024-03-10T01:30:00.0000000-05:00");
        Should.Throw<ArgumentException>(
            () => invalid.InTimeZone(zone, LocalTimeResolution.EarlierInstant));
    }

    [Fact]
    public void CalendarApplicationResolvesCalendarFieldsBeforeExactElapsedTime()
    {
        TimeZoneInfo zone = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
        LocalDateTime start = LocalDateTime.Parse("2024-03-09T03:30:00.0000000");
        var duration = new CalendarDuration(0, 1, new ElapsedDuration(TimeSpan.TicksPerHour));

        DateTimeOffset end = duration.ApplyInTimeZone(start, zone, LocalTimeResolution.Reject);

        end.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffffzzz")
            .ShouldBe("2024-03-10T04:30:00.0000000-04:00");

        new CalendarDuration(0, 0, new ElapsedDuration(TimeSpan.TicksPerHour))
            .ApplyInTimeZone(
                LocalDateTime.Parse("2024-03-10T01:30:00.0000000"),
                zone,
                LocalTimeResolution.Reject)
            .ToString("yyyy-MM-dd'T'HH:mm:ss.fffffffzzz")
            .ShouldBe("2024-03-10T03:30:00.0000000-04:00");
    }

    [Fact]
    public void CalendarDurationJsonUsesCanonicalText()
    {
        CalendarDuration value = CalendarDuration.Parse("P1Y1WT1.5S");
        string json = JsonSerializer.Serialize(value);

        json.ShouldBe("\"P12M7DT1.5S\"");
        JsonSerializer.Deserialize<CalendarDuration>(json).ShouldBe(value);
        Should.Throw<JsonException>(
            () => JsonSerializer.Deserialize<CalendarDuration>("\"P1.5D\""));
    }

    [Fact]
    public void ExactPeriodUsesHalfOpenNonOverlappingBoundaries()
    {
        DateTimeOffset start = DateTimeOffset.Parse("2024-01-01T00:00:00-07:00");
        ExactPeriod first = ExactPeriod.FromElapsed(start, new ElapsedDuration(10));
        ExactPeriod touching = ExactPeriod.FromElapsed(first.EndExclusive, new ElapsedDuration(10));
        ExactPeriod overlap = ExactPeriod.FromElapsed(start.AddTicks(9), new ElapsedDuration(10));
        var empty = new ExactPeriod(start, start);

        first.Contains(start).ShouldBeTrue();
        first.Contains(first.EndExclusive).ShouldBeFalse();
        first.Overlaps(touching).ShouldBeFalse();
        first.Overlaps(overlap).ShouldBeTrue();
        first.Overlaps(empty).ShouldBeFalse();
        empty.Overlaps(empty).ShouldBeFalse();
        first.Elapsed.Units.ShouldBe(10);
    }

    [Fact]
    public void ExactPeriodEqualityIncludesDeclaredOffsets()
    {
        DateTimeOffset start = DateTimeOffset.Parse("2024-01-01T00:00:00-07:00");
        ExactPeriod mountain = ExactPeriod.FromElapsed(start, new ElapsedDuration(10));
        ExactPeriod utc = ExactPeriod.FromElapsed(start.ToUniversalTime(), new ElapsedDuration(10));

        mountain.ShouldNotBe(utc);
        mountain.IsInstantEquivalentTo(utc).ShouldBeTrue();
        (mountain == utc).ShouldBeFalse();
        (mountain != utc).ShouldBeTrue();
    }

    [Fact]
    public void ExactPeriodCanonicalAndJsonRoundTripOffsetsAndTicks()
    {
        const string text =
            "2024-02-29T12:34:56.1234567-07:00/2024-02-29T13:34:56.1234568-07:00";
        ExactPeriod value = ExactPeriod.Parse(text);

        value.ToString().ShouldBe(text);
        ExactPeriod.TryParse(text, out ExactPeriod parsed).ShouldBeTrue();
        parsed.ShouldBe(value);
        JsonSerializer.Serialize(value).ShouldBe($"\"{text}\"");
        JsonSerializer.Deserialize<ExactPeriod>($"\"{text}\"").ShouldBe(value);
        ExactPeriod.TryParse(
            "2024-01-02T00:00:00.0000000+00:00/2024-01-01T00:00:00.0000000+00:00",
            out _).ShouldBeFalse();
    }

    [Fact]
    public void ExactPeriodCanBeBuiltFromCalendarDurationAndZonePolicy()
    {
        TimeZoneInfo zone = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
        LocalDateTime local = LocalDateTime.Parse("2024-03-09T03:30:00.0000000");
        var duration = new CalendarDuration(0, 1, ElapsedDuration.Zero);

        ExactPeriod period = ExactPeriod.FromCalendarDuration(
            local,
            duration,
            zone,
            LocalTimeResolution.Reject);

        period.Elapsed.Units.ShouldBe(23 * TimeSpan.TicksPerHour);
        Should.Throw<ArgumentOutOfRangeException>(
            () => ExactPeriod.FromElapsed(DateTimeOffset.UnixEpoch, new ElapsedDuration(-1)));
    }
}
