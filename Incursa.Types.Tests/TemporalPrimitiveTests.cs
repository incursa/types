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
using System.Text.Json;

namespace Incursa.Types.Tests;

public sealed class TemporalPrimitiveTests
{
    public static TheoryData<string, long, string> ElapsedVectors
    {
        get
        {
            var data = new TheoryData<string, long, string>();
            foreach (string line in ReadFixture("elapsed-duration-vectors.jsonl"))
            {
                using JsonDocument document = JsonDocument.Parse(line);
                JsonElement root = document.RootElement;
                data.Add(
                    root.GetProperty("input").GetString()!,
                    root.GetProperty("units").GetInt64(),
                    root.GetProperty("canonical").GetString()!);
            }

            return data;
        }
    }

    public static TheoryData<string> LocalDateTimeVectors
    {
        get
        {
            var data = new TheoryData<string>();
            foreach (string line in ReadFixture("local-date-time-vectors.jsonl"))
            {
                using JsonDocument document = JsonDocument.Parse(line);
                data.Add(document.RootElement.GetProperty("canonical").GetString()!);
            }

            return data;
        }
    }

    public static TheoryData<string, int, string> LocalOffsetVectors
    {
        get
        {
            var data = new TheoryData<string, int, string>();
            foreach (string line in ReadFixture("local-date-time-vectors.jsonl"))
            {
                using JsonDocument document = JsonDocument.Parse(line);
                JsonElement root = document.RootElement;
                if (root.TryGetProperty("offsetMinutes", out JsonElement offset))
                {
                    data.Add(
                        root.GetProperty("canonical").GetString()!,
                        offset.GetInt32(),
                        root.GetProperty("expectedUtc").GetString()!);
                }
            }

            return data;
        }
    }

    public static TheoryData<string, string, string> LocalZoneVectors
    {
        get
        {
            var data = new TheoryData<string, string, string>();
            foreach (string line in ReadFixture("local-date-time-vectors.jsonl"))
            {
                using JsonDocument document = JsonDocument.Parse(line);
                JsonElement root = document.RootElement;
                if (root.TryGetProperty("zoneId", out JsonElement zone))
                {
                    data.Add(
                        root.GetProperty("canonical").GetString()!,
                        zone.GetString()!,
                        root.GetProperty("zoneOutcome").GetString()!);
                }
            }

            return data;
        }
    }

    [Theory]
    [MemberData(nameof(ElapsedVectors))]
    public void ElapsedDurationVectorsPreserveExactUnitsAndCanonicalText(
        string input,
        long units,
        string canonical)
    {
        ElapsedDuration value = ElapsedDuration.Parse(input);

        value.Units.ShouldBe(units);
        value.ToString().ShouldBe(canonical);
        ElapsedDuration.Parse(canonical).ShouldBe(value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(10000000)]
    [InlineData(-10000000)]
    [InlineData(long.MaxValue)]
    [InlineData(long.MinValue)]
    public void ElapsedDurationRoundTripsTimeSpanExactly(long ticks)
    {
        TimeSpan timeSpan = TimeSpan.FromTicks(ticks);
        ElapsedDuration value = ElapsedDuration.FromTimeSpanExact(timeSpan);

        value.Units.ShouldBe(ticks);
        value.ToTimeSpanExact().ShouldBe(timeSpan);
    }

    [Fact]
    public void ElapsedDurationArithmeticIsCheckedAndOrderedByUnits()
    {
        var left = new ElapsedDuration(10);
        var right = new ElapsedDuration(3);

        (left + right).ShouldBe(new ElapsedDuration(13));
        (left - right).ShouldBe(new ElapsedDuration(7));
        (-right).ShouldBe(new ElapsedDuration(-3));
        (+right).ShouldBe(right);
        (left > right).ShouldBeTrue();
        (right < left).ShouldBeTrue();
        left.CompareTo(right).ShouldBeGreaterThan(0);
        Should.Throw<OverflowException>(() => new ElapsedDuration(long.MaxValue) + new ElapsedDuration(1));
        Should.Throw<OverflowException>(() => new ElapsedDuration(long.MinValue) - new ElapsedDuration(1));
        Should.Throw<OverflowException>(() => -new ElapsedDuration(long.MinValue));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("P1D")]
    [InlineData("P1Y")]
    [InlineData("PT1M")]
    [InlineData("pt1s")]
    [InlineData("+PT1S")]
    [InlineData("PT1")]
    [InlineData("PT.S")]
    [InlineData("PT1.S")]
    [InlineData("PT.1S")]
    [InlineData("PT1.12345678S")]
    [InlineData("PT922337203685.4775808S")]
    [InlineData("-PT922337203685.4775809S")]
    public void ElapsedDurationRejectsNonExactCalendarOrOutOfRangeText(string input)
    {
        ElapsedDuration.TryParse(input, out _).ShouldBeFalse();
        Should.Throw<FormatException>(() => ElapsedDuration.Parse(input));
    }

    [Fact]
    public void ElapsedDurationFormattingAndJsonAreCanonical()
    {
        var value = new ElapsedDuration(-12345678);
        Span<char> buffer = stackalloc char[32];

        value.TryFormat(buffer, out int written, "G", CultureInfo.GetCultureInfo("fr-FR"))
            .ShouldBeTrue();
        buffer[..written].ToString().ShouldBe("-PT1.2345678S");
        value.TryFormat(buffer, out _, "F", null).ShouldBeFalse();
        Should.Throw<FormatException>(() => value.ToString("F", null));
        string json = JsonSerializer.Serialize(value);
        json.ShouldBe("\"-PT1.2345678S\"");
        JsonSerializer.Deserialize<ElapsedDuration>(json).ShouldBe(value);
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<ElapsedDuration>("\"P1D\""));
    }

    [Theory]
    [MemberData(nameof(LocalDateTimeVectors))]
    public void LocalDateTimeVectorsRoundTripCanonicalFields(string canonical)
    {
        LocalDateTime value = LocalDateTime.Parse(canonical);

        value.ToString().ShouldBe(canonical);
        value.Value.Kind.ShouldBe(DateTimeKind.Unspecified);
        LocalDateTime.Parse(value.ToString()).ShouldBe(value);
        LocalDateTime.FromTicks(value.Ticks).ShouldBe(value);
    }

    [Fact]
    public void LocalDateTimeComponentConstructorPreservesHundredNanosecondPrecision()
    {
        var value = new LocalDateTime(2024, 2, 29, 23, 59, 58, 1234567);

        value.Year.ShouldBe(2024);
        value.Month.ShouldBe(2);
        value.Day.ShouldBe(29);
        value.Hour.ShouldBe(23);
        value.Minute.ShouldBe(59);
        value.Second.ShouldBe(58);
        value.SubsecondTicks.ShouldBe(1234567);
        value.ToString().ShouldBe("2024-02-29T23:59:58.1234567");
    }

    [Theory]
    [InlineData("")]
    [InlineData("2024-01-01")]
    [InlineData("2024-01-01T00:00:00")]
    [InlineData("2024-01-01T00:00:00.000000")]
    [InlineData("2024-01-01T00:00:00.00000000")]
    [InlineData("2024-01-01T24:00:00.0000000")]
    [InlineData("2024-01-01T23:59:60.0000000")]
    [InlineData("2023-02-29T00:00:00.0000000")]
    [InlineData("0000-01-01T00:00:00.0000000")]
    [InlineData("2024-01-01T00:00:00.0000000Z")]
    [InlineData("2024-01-01T00:00:00.0000000-07:00")]
    [InlineData(" 2024-01-01T00:00:00.0000000")]
    public void LocalDateTimeRejectsInvalidNonCanonicalOrZonedText(string input)
    {
        LocalDateTime.TryParse(input, out _).ShouldBeFalse();
        Should.Throw<FormatException>(() => LocalDateTime.Parse(input));
    }

    [Fact]
    public void LocalDateTimeRejectsDateTimeWithImplicitZoneSemantics()
    {
        Should.Throw<ArgumentException>(() => new LocalDateTime(DateTime.UtcNow));
        Should.Throw<ArgumentException>(() => new LocalDateTime(DateTime.Now));
        Should.Throw<ArgumentOutOfRangeException>(() => new LocalDateTime(2024, 1, 1, 0, 0, 0, -1));
        Should.Throw<ArgumentOutOfRangeException>(
            () => new LocalDateTime(2024, 1, 1, 0, 0, 0, (int)TimeSpan.TicksPerSecond));
        Should.Throw<ArgumentOutOfRangeException>(() => LocalDateTime.FromTicks(-1));
        Should.Throw<ArgumentOutOfRangeException>(() => LocalDateTime.FromTicks(long.MaxValue));
    }

    [Fact]
    public void LocalDateTimeArithmeticUsesExactElapsedTicksAndChecksRange()
    {
        LocalDateTime start = LocalDateTime.Parse("2024-02-28T23:59:59.9999999");
        var oneTick = new ElapsedDuration(1);

        LocalDateTime leapDay = start + oneTick;
        leapDay.ToString().ShouldBe("2024-02-29T00:00:00.0000000");
        (leapDay - start).ShouldBe(oneTick);
        (leapDay - oneTick).ShouldBe(start);
        Should.Throw<OverflowException>(() => LocalDateTime.MaxValue + oneTick);
        Should.Throw<OverflowException>(() => LocalDateTime.MinValue - oneTick);
        Should.Throw<OverflowException>(
            () => LocalDateTime.MinValue.Subtract(new ElapsedDuration(long.MinValue)));
    }

    [Theory]
    [MemberData(nameof(LocalOffsetVectors))]
    public void ExplicitOffsetResolutionPreservesFieldsAndDefinesInstant(
        string localText,
        int offsetMinutes,
        string expectedUtc)
    {
        LocalDateTime local = LocalDateTime.Parse(localText);
        TimeSpan offset = TimeSpan.FromMinutes(offsetMinutes);
        DateTimeOffset resolved = local.WithOffset(offset);

        resolved.Offset.ShouldBe(offset);
        resolved.UtcDateTime.ShouldBe(
            DateTime.ParseExact(
                expectedUtc,
                "yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal));
        Should.Throw<ArgumentException>(() => local.WithOffset(TimeSpan.FromHours(15)));
        Should.Throw<ArgumentException>(() => local.WithOffset(TimeSpan.FromSeconds(1)));
    }

    [Theory]
    [MemberData(nameof(LocalZoneVectors))]
    public void NamedZoneResolutionUsesPortableDstOutcomeFixture(
        string localText,
        string zoneId,
        string outcome)
    {
        TimeZoneInfo zone = TimeZoneInfo.FindSystemTimeZoneById(zoneId);
        LocalDateTime local = LocalDateTime.Parse(localText);

        if (outcome is "ambiguous" or "invalid")
        {
            Should.Throw<ArgumentException>(() => local.InTimeZone(zone));
            return;
        }

        DateTimeOffset resolved = local.InTimeZone(zone);
        resolved.UtcDateTime.ShouldBe(
            DateTime.ParseExact(
                outcome,
                "yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal));
    }

    [Fact]
    public void LocalDateTimeOrdersFieldsWithoutHostTimezoneInterpretation()
    {
        LocalDateTime earlier = LocalDateTime.Parse("2024-01-01T00:00:00.0000000");
        LocalDateTime later = LocalDateTime.Parse("2024-01-01T00:00:00.0000001");

        (earlier < later).ShouldBeTrue();
        (later > earlier).ShouldBeTrue();
        earlier.CompareTo(later).ShouldBeLessThan(0);
        LocalDateTime.MinValue.ShouldBe(default);
        LocalDateTime.MaxValue.Ticks.ShouldBe(DateTime.MaxValue.Ticks);
    }

    [Fact]
    public void LocalDateTimeFormattingAndJsonAreCanonical()
    {
        LocalDateTime value = LocalDateTime.Parse("2024-02-29T12:34:56.1234567");
        Span<char> buffer = stackalloc char[27];

        value.TryFormat(buffer, out int written, "G", CultureInfo.GetCultureInfo("de-DE"))
            .ShouldBeTrue();
        buffer[..written].ToString().ShouldBe(value.ToString());
        value.TryFormat(buffer, out _, "O", null).ShouldBeFalse();
        Should.Throw<FormatException>(() => value.ToString("O", null));
        string json = JsonSerializer.Serialize(value);
        json.ShouldBe("\"2024-02-29T12:34:56.1234567\"");
        JsonSerializer.Deserialize<LocalDateTime>(json).ShouldBe(value);
        Should.Throw<JsonException>(
            () => JsonSerializer.Deserialize<LocalDateTime>("\"2024-02-29T12:34:56Z\""));
    }

    private static IEnumerable<string> ReadFixture(string name)
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Fixtures", name);
        return File.ReadLines(path);
    }
}
