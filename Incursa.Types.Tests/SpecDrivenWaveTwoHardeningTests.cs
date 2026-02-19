using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Incursa.Types.Tests;

public partial class SpecDrivenWaveTwoHardeningTests
{
    [Fact]
    public void JsonContext_JsonTypeInfoOverloads_RoundTripAndMutate()
    {
        var payload = new WaveTwoPayload("sam", 42);
        var context = JsonContext.FromObject(payload, WaveTwoContext.Default.WaveTwoPayload);

        WaveTwoPayload readBack = context.GetData(WaveTwoContext.Default.WaveTwoPayload);
        readBack.ShouldBe(payload);

        context.SetData("name", "alex", WaveTwoContext.Default.String);
        context.SetData("score", 99, WaveTwoContext.Default.Int32);
        context.SetData("meta", payload, WaveTwoContext.Default.WaveTwoPayload);

        context.GetData<string>("name", WaveTwoContext.Default.String).ShouldBe("alex");
        context.GetData<int>("score", WaveTwoContext.Default.Int32).ShouldBe(99);
        context.GetData<WaveTwoPayload>("meta", WaveTwoContext.Default.WaveTwoPayload).ShouldBe(payload);
    }

    [Fact]
    public void JsonContext_FromJson_HandlesNullAndValidObject()
    {
        JsonContext.FromJson(null).ShouldBeNull();

        var jsonObject = new System.Text.Json.Nodes.JsonObject
        {
            ["kind"] = "sample",
            ["count"] = 4,
        };

        JsonContext context = JsonContext.FromJson(jsonObject)!.Value;
        context.GetData<string>("kind").ShouldBe("sample");
        context.GetData<int>("count").ShouldBe(4);
    }

    [Fact]
    public void Maybe_Constructors_And_ImplicitConversions_WorkAcrossNullBehavior()
    {
        Maybe<string> none = new(null!, nullIsNone: true);
        none.HasValue.ShouldBeFalse();

        Maybe<string> someNull = new(null!, nullIsNone: false);
        someNull.HasValue.ShouldBeTrue();
        someNull.Value.ShouldBeNull();

        Maybe<int> fromImplicit = 42;
        fromImplicit.HasValue.ShouldBeTrue();
        fromImplicit.Value.ShouldBe(42);

        Maybe<object> boxed = ConvertToObjectMaybe(fromImplicit);
        boxed.HasValue.ShouldBeTrue();
        boxed.Value.ShouldBe(42);

        Maybe<object> fromNullable = ConvertToObjectMaybe((Maybe<int>?)null);
        fromNullable.HasValue.ShouldBeFalse();
    }

    [Fact]
    public void Maybe_SelectMany_Match_And_DefaultPaths_AreConsistent()
    {
        Maybe<int> some = new(5);
        Maybe<int> none = Maybe<int>.None;

        int matchedNone = none.Match(() => -1, v => v);
        matchedNone.ShouldBe(-1);

        Maybe<int> projected = some.SelectMany(v => new Maybe<int>(v + 1));
        projected.HasValue.ShouldBeTrue();
        projected.Value.ShouldBe(6);

        Maybe<string> combined = some.SelectMany(
            v => new Maybe<int>(v * 2),
            (left, right) => $"{left}:{right}");

        combined.HasValue.ShouldBeTrue();
        combined.Value.ShouldBe("5:10");

        Maybe<string> noneCombined = none.SelectMany(
            v => new Maybe<int>(v * 2),
            (left, right) => $"{left}:{right}");

        noneCombined.HasValue.ShouldBeFalse();
        noneCombined.Or("fallback").ShouldBe("fallback");
        noneCombined.GetValueOrDefault("fallback").ShouldBe("fallback");
        noneCombined.GetValueOrDefault().ShouldBeNull();
    }

    [Fact]
    public void CidrRange_ParseTryParse_Contains_AndConverters_CoverEdgeCases()
    {
        CidrRange ipv4 = CidrRange.Parse("192.168.10.0/25");
        ipv4.Contains(IpAddress.Parse("192.168.10.42")).ShouldBeTrue();
        ipv4.Contains(IpAddress.Parse("192.168.10.200")).ShouldBeFalse();

        CidrRange ipv6 = CidrRange.Parse("2001:db8::/64");
        ipv6.Contains(IpAddress.Parse("2001:db8::1")).ShouldBeTrue();
        ipv6.Contains(IpAddress.Parse("2001:db9::1")).ShouldBeFalse();

        CidrRange.TryParse("10.0.0.0/8", out _).ShouldBeTrue();
        CidrRange.TryParse("bad-cidr", out _).ShouldBeFalse();
        CidrRange.TryParse("10.0.0.0/33", out _).ShouldBeFalse();
        CidrRange.TryParse("2001:db8::/129", out _).ShouldBeFalse();
        CidrRange.TryParse("10.0.0.0/-1", out _).ShouldBeFalse();

        Should.Throw<ArgumentException>(() => CidrRange.Parse("10.0.0.0"));
        Should.Throw<ArgumentException>(() => CidrRange.Parse("10.0.0.0/not-a-prefix"));
        Should.Throw<ArgumentOutOfRangeException>(() => CidrRange.Parse("10.0.0.0/33"));

        TypeConverter converter = TypeDescriptor.GetConverter(typeof(CidrRange));
        CidrRange fromConverter = (CidrRange)converter.ConvertFrom("10.0.0.0/8")!;
        converter.ConvertTo(fromConverter, typeof(string)).ShouldBe("10.0.0.0/8");
        Should.Throw<NotSupportedException>(() => converter.ConvertFrom("bad-cidr"));

        JsonSerializer.Deserialize<CidrRange>("\"10.0.0.0/8\"").ToString().ShouldBe("10.0.0.0/8");
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<CidrRange>("\"bad-cidr\""));
    }

    [Fact]
    public void Money_PercentageAndFormattingEdgeCases_AreDeterministic()
    {
        Money.CalculateRelativePercentage(null, null).ShouldBe(Percentage.Hundred);
        Money.CalculateRelativePercentage(new Money(10), Money.Zero).ShouldBe(Percentage.Zero);
        Money.CalculateRelativePercentage(new Money(5), new Money(20)).Value.ShouldBe(0.25m);

        new Money(20).CalculateRelativePercentageFrom(Money.Zero).ShouldBe(Percentage.Zero);
        new Money(20).CalculateRelativePercentageFrom(new Money(40)).Value.ShouldBe(0.5m);
        Money.Zero.CalculateRelativePercentageTo(new Money(20)).ShouldBe(Percentage.Zero);
        new Money(20).CalculateRelativePercentageTo(new Money(10)).Value.ShouldBe(0.5m);

        Money.Parse("1,23", new CultureInfo("fr-FR")).Value.ShouldBe(1.23m);
        Money.TryParse("1,23", new CultureInfo("fr-FR"), out Money parsedFr).ShouldBeTrue();
        parsedFr.Value.ShouldBe(1.23m);

        new Money(-1234.5m).ToAccounting(flipSign: true, includeCurrencySymbol: false).ShouldBe("1,234.50");
        new Money(1234.5m).ToPlainNumberString().ShouldBe("1234.50");

        Should.Throw<ArgumentException>(() => new Money(1).CompareTo("x"));
    }

    [Fact]
    public void UsaState_ConverterAndJsonPropertyName_RoundTrip()
    {
        TypeConverter converter = TypeDescriptor.GetConverter(typeof(UsaState));
        UsaState parsed = (UsaState)converter.ConvertFrom("tx")!;
        parsed.ShouldBe(UsaState.Texas);
        converter.ConvertTo(parsed, typeof(string)).ShouldBe("TX");

        var map = new Dictionary<UsaState, int> { [UsaState.California] = 1, [UsaState.NewYork] = 2 };
        string json = JsonSerializer.Serialize(map);
        Dictionary<UsaState, int> roundTrip = JsonSerializer.Deserialize<Dictionary<UsaState, int>>(json)!;

        roundTrip[UsaState.California].ShouldBe(1);
        roundTrip[UsaState.NewYork].ShouldBe(2);

        UsaState.Parse("ca", provider: null).ShouldBe(UsaState.California);
    }

    private static Maybe<object> ConvertToObjectMaybe(Maybe<int> value)
    {
        return !value.HasValue ? Maybe<object>.None : new Maybe<object>(value.Value);
    }

    private static Maybe<object> ConvertToObjectMaybe(Maybe<int>? value)
    {
        return value is null || !value.Value.HasValue ? Maybe<object>.None : new Maybe<object>(value.Value.Value);
    }

    private sealed record WaveTwoPayload(string Name, int Age);

    [JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
    [JsonSerializable(typeof(WaveTwoPayload))]
    [JsonSerializable(typeof(string))]
    [JsonSerializable(typeof(int))]
    private partial class WaveTwoContext : JsonSerializerContext;
}
