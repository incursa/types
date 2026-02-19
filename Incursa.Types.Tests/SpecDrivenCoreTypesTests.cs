using System.Text.Json;

namespace Incursa.Types.Tests;

public class SpecDrivenCoreTypesTests
{
    [Fact]
    public void Percentage_ParseScaled_InterpretsScaledInput()
    {
        var pct = Percentage.ParseScaled("12.5");

        Assert.Equal(new Percentage(0.125m), pct);
        Assert.Equal(12.50m, pct.ScaledValue);
    }

    [Fact]
    public void Percentage_TryParseScaled_InvalidInput_ReturnsFalse()
    {
        Assert.False(Percentage.TryParseScaled("x", out var parsed));
        Assert.Equal(default, parsed);
    }

    [Fact]
    public void Percentage_JsonConverter_WritesInvariantRawFraction()
    {
        var pct = new Percentage(0.12345m);

        var json = JsonSerializer.Serialize(pct);

        Assert.Equal("\"0.1234\"", json);
        Assert.Equal(pct, JsonSerializer.Deserialize<Percentage>(json));
    }

    [Fact]
    public void Duration_Zero_ToString_IsP0D()
    {
        var duration = new Duration(null, null, null, null, null, null, null);

        Assert.Equal("P0D", duration.ToString());
    }

    [Fact]
    public void Duration_Parse_Invalid_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => Duration.Parse("not-a-duration"));
    }

    [Fact]
    public void Duration_TryParse_Invalid_ReturnsNullAndFalse()
    {
        Assert.Null(Duration.TryParse("not-a-duration"));
        Assert.False(Duration.TryParse("not-a-duration", out var parsed));
        Assert.Equal(default, parsed);
    }

    [Fact]
    public void Period_Parse_Invalid_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => Period.Parse("2025-01-01"));
    }

    [Fact]
    public void Period_TryParse_Invalid_ReturnsNullAndFalse()
    {
        Assert.Null(Period.TryParse("2025-01-01"));
        Assert.False(Period.TryParse("2025-01-01", out var parsed));
        Assert.Equal(default, parsed);
    }

    [Fact]
    public void Period_ClampDate_DefaultUsesEndExclusive()
    {
        var start = new DateTimeOffset(2025, 1, 1, 23, 0, 0, TimeSpan.Zero);
        var period = new Period(start, start.AddHours(2));

        var clamped = period.ClampDate();

        Assert.Equal(new DateOnly(2025, 1, 2), clamped);
    }

    [Fact]
    public void RecurringPeriod_Parse_Invalid_ReturnsNullAndFalse()
    {
        Assert.Null(RecurringPeriod.TryParse("not-a-cron"));
        Assert.False(RecurringPeriod.TryParse("not-a-cron", out var parsed));
        Assert.Equal(default, parsed);
    }

    [Fact]
    public void RecurringPeriod_Parse_Invalid_Throws()
    {
        Assert.Throws<Cronos.CronFormatException>(() => RecurringPeriod.Parse("not-a-cron"));
    }

    [Fact]
    public void FastId_TryParse_InvalidCharacter_ReturnsFalse()
    {
        Assert.False(FastId.TryParse("*", out var parsed));
        Assert.Equal(default, parsed);
    }

    [Fact]
    public void FastId_Parse_Invalid_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => FastId.Parse("*"));
    }

    [Fact]
    public void FastId_New_ReturnsNonEmptyValue()
    {
        var id = FastId.New();

        Assert.NotEqual(0, id.Value);
        Assert.NotEqual("0", id.Encoded);
    }

    [Fact]
    public void FastId_JsonConverter_RoundTripsEncodedValue()
    {
        var id = FastId.New();

        var json = JsonSerializer.Serialize(id);
        var roundTrip = JsonSerializer.Deserialize<FastId>(json);

        Assert.Equal(id, roundTrip);
    }
}
