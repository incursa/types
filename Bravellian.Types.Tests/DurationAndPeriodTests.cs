using System.Text;
using Bravellian;
using Cronos;
using Xunit;

public class DurationAndPeriodTests
{
    [Fact]
    public void Duration_Parse_RoundTripsCommonPatterns()
    {
        string[] samples = ["P1D", "PT2H30M", "P3Y6M4DT12H30M5S", "P1W", "P0.5D", "PT0.25H"];

        foreach (var sample in samples)
        {
            var parsed = Duration.Parse(sample);
            Assert.Equal(sample, parsed.ToString());
        }
    }

    [Fact]
    public void Duration_Calculate_AppliesFractionalUnits()
    {
        var start = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var duration = new Duration(null, null, null, 1.5, null, null, null);

        var calculated = duration.Calculate(start);

        Assert.Equal(start.AddDays(1).AddHours(12), calculated);
    }

    [Fact]
    public void Period_ParseAndContains()
    {
        var start = new DateTimeOffset(2025, 1, 2, 8, 0, 0, TimeSpan.Zero);
        var duration = new Duration(null, null, null, 0, 2, 15, 0);
        var period = new Period(start, duration);

        var text = period.ToString();
        var parsed = Period.Parse(text);

        Assert.True(parsed.Contains(start.AddHours(1)));
        Assert.False(parsed.Contains(start.AddHours(3)));
    }

    [Fact]
    public void Period_Overlaps_DetectsIntersection()
    {
        var baseStart = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var first = new Period(baseStart, new Duration(null, null, null, 1, 0, 0, 0));
        var second = new Period(baseStart.AddHours(12), new Duration(null, null, null, 1, 0, 0, 0));

        Assert.True(first.Overlaps(second));
    }

    [Fact]
    public void RecurringPeriod_GeneratesWindow()
    {
        var cron = CronExpression.Parse("0 0 * * *"); // daily UTC midnight
        var recurring = new RecurringPeriod(cron);

        var window = recurring.GetPeriod(DateTime.UtcNow);

        Assert.NotEqual(default, window);
        Assert.True(window.EndExclusive > window.StartInclusive);
    }
}
