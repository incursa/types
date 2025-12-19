using System.Text.Json;
using Bravellian;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Xunit;

namespace Bravellian.Types.Tests;

public class SerializationAndEfSamplesTests
{
    private record AggregateSample(FastId Id, Money Amount, Percentage Rate, VirtualPath Path, UsaState State);

    [Fact]
    public void JsonConverters_RoundTripPayload()
    {
        var fastId = FastId.FromGuidWithinTimestampRange(Guid.Parse("8c5018c1-47d3-4ec8-8c19-73b58ff0ae9c"), DateTimeOffset.UtcNow);
        var sample = new AggregateSample(fastId, new Money(12.34m), new Percentage(7.5m), new VirtualPath("assets/logo.png"), UsaState.California);

        var json = JsonSerializer.Serialize(sample);
        using var document = JsonDocument.Parse(json);

        Assert.Equal(sample.Id.Encoded, document.RootElement.GetProperty("Id").GetString());
        Assert.Equal(sample.Amount, Money.Parse(document.RootElement.GetProperty("Amount").GetString()!));
        Assert.Equal(sample.Rate, Percentage.Parse(document.RootElement.GetProperty("Rate").GetString()!));
        Assert.Equal(sample.Path.ToString(), document.RootElement.GetProperty("Path").GetString());
        Assert.Equal(sample.State.Value, document.RootElement.GetProperty("State").GetString());

        var roundTrip = JsonSerializer.Deserialize<AggregateSample>(json);

        Assert.NotNull(roundTrip);
        Assert.Equal(sample.Id, roundTrip!.Id);
        Assert.Equal(sample.Amount, roundTrip.Amount);
        Assert.Equal(sample.Rate, roundTrip.Rate);
        Assert.Equal(sample.Path, roundTrip.Path);
        Assert.Equal(sample.State, roundTrip.State);
    }

    [Fact]
    public void EfCoreValueConverters_RoundTrip()
    {
        var fastId = FastId.FromGuidWithinTimestampRange(Guid.Parse("f12e5509-fcb4-49a6-af04-665a901847bd"), DateTimeOffset.UtcNow);

        var fastIdConverter = new ValueConverter<FastId, string>(
            v => v.Encoded,
            v => FastId.Parse(v));

        var moneyConverter = new ValueConverter<Money, decimal>(
            v => v.Value,
            v => new Money(v));

        var usaStateConverter = new ValueConverter<UsaState, string>(
            v => v.Value,
            v => UsaState.From(v));

        var percentageConverter = new ValueConverter<Percentage, decimal>(
            v => v.Value,
            v => new Percentage(v));

        var fastIdProvider = fastIdConverter.ConvertToProvider(fastId);
        var moneyProvider = moneyConverter.ConvertToProvider(new Money(42.10m));
        var usaStateProvider = usaStateConverter.ConvertToProvider(UsaState.Texas);
        var percentageProvider = percentageConverter.ConvertToProvider(new Percentage(12.34m));

        Assert.Equal(fastId, fastIdConverter.ConvertFromProvider(fastIdProvider!));
        Assert.Equal(new Money(42.10m), moneyConverter.ConvertFromProvider(moneyProvider!));
        Assert.Equal(UsaState.Texas, usaStateConverter.ConvertFromProvider(usaStateProvider!));
        Assert.Equal(new Percentage(12.34m), percentageConverter.ConvertFromProvider(percentageProvider!));
    }
}
