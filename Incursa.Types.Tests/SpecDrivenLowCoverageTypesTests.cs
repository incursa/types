using System.ComponentModel;
using System.Reflection;
using System.Text.Json;

namespace Incursa.Types.Tests;

public class SpecDrivenLowCoverageTypesTests
{
    [Theory]
    [InlineData(" usd ", "USD", 840, 2)]
    [InlineData("jpy", "JPY", 392, 0)]
    [InlineData("bhd", "BHD", 48, 3)]
    public void CurrencyCode_Parse_NormalizesAndPopulatesMetadata(string input, string code, int numericCode, int minorUnit)
    {
        CurrencyCode parsed = CurrencyCode.Parse(input);

        parsed.Code.ShouldBe(code);
        parsed.NumericCode.ShouldBe(numericCode);
        parsed.MinorUnit.ShouldBe(minorUnit);
        parsed.ToString().ShouldBe(code);
    }

    [Fact]
    public void CurrencyCode_TryParse_And_Format_APIs_AreConsistent()
    {
        CurrencyCode.TryParse("eur", out CurrencyCode parsed).ShouldBeTrue();
        CurrencyCode.TryParse("eur", provider: null, out CurrencyCode parsedWithProvider).ShouldBeTrue();
        parsed.ShouldBe(parsedWithProvider);

        Span<char> tooSmall = stackalloc char[2];
        parsed.TryFormat(tooSmall, out int tooSmallWritten, default, null).ShouldBeFalse();
        tooSmallWritten.ShouldBe(0);

        Span<char> buffer = stackalloc char[3];
        parsed.TryFormat(buffer, out int written, default, null).ShouldBeTrue();
        written.ShouldBe(3);
        new string(buffer).ShouldBe("EUR");
    }

    [Fact]
    public void CurrencyCode_Converters_RoundTrip_And_RejectInvalidInput()
    {
        TypeConverter converter = TypeDescriptor.GetConverter(typeof(CurrencyCode));

        CurrencyCode fromConverter = (CurrencyCode)converter.ConvertFrom("usd")!;
        converter.ConvertTo(fromConverter, typeof(string)).ShouldBe("USD");
        Should.Throw<NotSupportedException>(() => converter.ConvertFrom("not-a-currency"));

        JsonSerializer.Deserialize<CurrencyCode>("\"usd\"").Code.ShouldBe("USD");
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<CurrencyCode>("\"not-a-currency\""));
    }

    [Fact]
    public void CountryCode_Parse_AcceptsAlpha2_Alpha3_AndCultureTags()
    {
        CountryCode fromTwo = CountryCode.Parse("us");
        CountryCode fromThree = CountryCode.Parse("USA");
        CountryCode fromCulture = CountryCode.Parse("en-US");

        fromTwo.TwoLetterCode.ShouldBe("US");
        fromTwo.ThreeLetterCode.ShouldBe("USA");
        fromTwo.EnglishName.ShouldContain("United States");
        fromThree.ShouldBe(fromTwo);
        fromCulture.ShouldBe(fromTwo);
        fromTwo.ToString().ShouldBe("US");
    }

    [Fact]
    public void CountryCode_TryParse_Format_And_Converters_BehaveAsSpecified()
    {
        CountryCode.TryParse("gbr", out CountryCode parsed).ShouldBeTrue();
        parsed.TwoLetterCode.ShouldBe("GB");
        CountryCode.TryParse("bad-input", out _).ShouldBeFalse();
        CountryCode.TryParse("bad-input", provider: null, out _).ShouldBeFalse();

        Span<char> buffer = stackalloc char[2];
        parsed.TryFormat(buffer, out int written, default, null).ShouldBeTrue();
        written.ShouldBe(2);
        new string(buffer).ShouldBe("GB");

        Span<char> tooSmall = stackalloc char[1];
        parsed.TryFormat(tooSmall, out int tooSmallWritten, default, null).ShouldBeFalse();
        tooSmallWritten.ShouldBe(0);

        TypeConverter converter = TypeDescriptor.GetConverter(typeof(CountryCode));
        CountryCode fromConverter = (CountryCode)converter.ConvertFrom("can")!;
        fromConverter.TwoLetterCode.ShouldBe("CA");
        converter.ConvertTo(fromConverter, typeof(string)).ShouldBe("CA");
        Should.Throw<NotSupportedException>(() => converter.ConvertFrom("invalid-country"));

        JsonSerializer.Deserialize<CountryCode>("\"deu\"").TwoLetterCode.ShouldBe("DE");
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<CountryCode>("\"invalid-country\""));
    }

    [Fact]
    public void IpAddress_Parse_TryParse_And_Converters_HandleV4AndV6()
    {
        IpAddress parsedV4 = IpAddress.Parse(" 192.168.1.20 ");
        parsedV4.IsIPv4.ShouldBeTrue();
        parsedV4.IsIPv6.ShouldBeFalse();
        parsedV4.Value.ShouldBe("192.168.1.20");

        IpAddress parsedV6 = IpAddress.Parse("2001:0db8::1");
        parsedV6.IsIPv4.ShouldBeFalse();
        parsedV6.IsIPv6.ShouldBeTrue();
        parsedV6.ToString().ShouldBe("2001:db8::1");

        IpAddress.TryParse("127.0.0.1", out IpAddress tryParsed).ShouldBeTrue();
        tryParsed.Value.ShouldBe("127.0.0.1");
        IpAddress.TryParse("127.0.0.1", provider: null, out _).ShouldBeTrue();
        IpAddress.TryParse("not-an-ip", out _).ShouldBeFalse();
        IpAddress.TryParse("not-an-ip", provider: null, out _).ShouldBeFalse();
    }

    [Fact]
    public void IpAddress_Converters_RoundTrip_And_RejectInvalidInput()
    {
        TypeConverter converter = TypeDescriptor.GetConverter(typeof(IpAddress));
        IpAddress parsed = (IpAddress)converter.ConvertFrom("10.1.2.3")!;
        converter.ConvertTo(parsed, typeof(string)).ShouldBe("10.1.2.3");
        Should.Throw<NotSupportedException>(() => converter.ConvertFrom("999.999.999.999"));

        JsonSerializer.Deserialize<IpAddress>("\"10.1.2.3\"").Value.ShouldBe("10.1.2.3");
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<IpAddress>("\"999.999.999.999\""));
    }

    [Fact]
    public void PhoneNumber_Parse_TryParse_Compare_And_Converters_AreConsistent()
    {
        PhoneNumber parsed = PhoneNumber.Parse("+1 (415) 555-2671");
        parsed.Value.ShouldBe("+14155552671");
        parsed.Number.ShouldBe("+14155552671");
        parsed.RegionCode.ShouldBe("US");

        PhoneNumber.Parse("+1 (415) 555-2671", provider: null).ShouldBe(parsed);
        PhoneNumber.TryParse("+1 (415) 555-2671", out PhoneNumber parsedTry).ShouldBeTrue();
        parsedTry.ShouldBe(parsed);
        PhoneNumber.TryParse("+1 (415) 555-2671", provider: null, out PhoneNumber parsedTryWithProvider).ShouldBeTrue();
        parsedTryWithProvider.ShouldBe(parsed);

        PhoneNumber.TryParse("abc", out _).ShouldBeFalse();
        PhoneNumber.TryParse((string?)null).ShouldBeNull();
        Should.Throw<FormatException>(() => PhoneNumber.Parse("abc"));

        TypeConverter converter = TypeDescriptor.GetConverter(typeof(PhoneNumber));
        PhoneNumber fromConverter = (PhoneNumber)converter.ConvertFrom("+1 415 555 2671")!;
        converter.ConvertTo(fromConverter, typeof(string)).ShouldBe("+14155552671");
        Should.Throw<FormatException>(() => converter.ConvertFrom("not-a-phone"));

        JsonSerializer.Deserialize<PhoneNumber>("\"+14155552671\"").ShouldBe(parsed);
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<PhoneNumber>("\"not-a-phone\""));

        PhoneNumber smaller = PhoneNumber.Parse("+12025550101");
        (smaller < parsed).ShouldBeTrue();
        (parsed > smaller).ShouldBeTrue();
    }

    [Fact]
    public void PhoneNumber_GenerateRandom_ProducesParsableValue()
    {
        PhoneNumber generated = PhoneNumber.GenerateRandom();

        generated.Value.ShouldNotBeNullOrWhiteSpace();
        PhoneNumber.TryParse(generated.Value, out _).ShouldBeTrue();
    }

    [Fact]
    public void ShortCode_Parse_Generate_Format_And_Converters_AreConsistent()
    {
        ShortCode parsed = ShortCode.Parse("ab-cd-ef-gh");
        parsed.RawCode.ShouldBe("ABCDEFGH");
        parsed.ToString().ShouldBe("ABCDEFGH");
        parsed.FormattedCode.ShouldBe("ABCD-EFGH");
        parsed.IsEmpty.ShouldBeFalse();

        ShortCode.TryParse("ab-cd-ef-gh", out ShortCode tryParsed).ShouldBeTrue();
        tryParsed.ShouldBe(parsed);
        ShortCode.TryParse("abc?", out _).ShouldBeFalse();
        ShortCode.TryParse("   ", out _).ShouldBeFalse();
        Should.Throw<ArgumentException>(() => ShortCode.Parse("abc?"));

        ShortCode generated = ShortCode.Generate(12);
        generated.RawCode.Length.ShouldBe(12);
        generated.RawCode.All(c => AllowedShortCodeCharacters.Contains(c)).ShouldBeTrue();
        Should.Throw<ArgumentOutOfRangeException>(() => ShortCode.Generate(3));
        Should.Throw<ArgumentOutOfRangeException>(() => ShortCode.Generate(33));

        TypeConverter converter = TypeDescriptor.GetConverter(typeof(ShortCode));
        ShortCode fromConverter = (ShortCode)converter.ConvertFrom("abcd-efgh")!;
        converter.ConvertTo(fromConverter, typeof(string)).ShouldBe("ABCDEFGH");

        JsonSerializer.Deserialize<ShortCode>("\"abcd-efgh\"").RawCode.ShouldBe("ABCDEFGH");
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<ShortCode>("123"));
    }

    [Fact]
    public void UsaState_Converters_And_Parse_APIs_RoundTrip()
    {
        UsaState parsed = UsaState.Parse("ny");
        parsed.ShouldBe(UsaState.NewYork);
        parsed.DisplayName.ShouldBe("New York");
        parsed.ToString().ShouldBe("NY");
        UsaState.From("ny").ShouldBe(UsaState.NewYork);

        UsaState.TryParse("wa", provider: null, out UsaState parsedWithProvider).ShouldBeTrue();
        parsedWithProvider.ShouldBe(UsaState.Washington);
        Should.Throw<ArgumentOutOfRangeException>(() => UsaState.Parse("xx"));

        TypeConverter converter = TypeDescriptor.GetConverter(typeof(UsaState));
        UsaState fromConverter = (UsaState)converter.ConvertFrom("ca")!;
        fromConverter.ShouldBe(UsaState.California);
        converter.ConvertTo(fromConverter, typeof(string)).ShouldBe("CA");
        Should.Throw<FormatException>(() => converter.ConvertFrom("xx"));

        JsonSerializer.Deserialize<UsaState>("\"tx\"").ShouldBe(UsaState.Texas);
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<UsaState>("\"xx\""));
    }

    [Fact]
    public void UsaState_MatchAndTryMatch_ResolveAllKnownStates_AndRejectUnknown()
    {
        MethodInfo actionTryMatch = typeof(UsaState)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Single(m => string.Equals(m.Name, nameof(UsaState.TryMatch), StringComparison.Ordinal) && !m.IsGenericMethod && m.GetParameters().Length == 51);

        MethodInfo funcMatch = typeof(UsaState)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Single(m => string.Equals(m.Name, nameof(UsaState.Match), StringComparison.Ordinal) && m.IsGenericMethod && m.GetParameters().Length == 51)
            .MakeGenericMethod(typeof(string));

        foreach (UsaState state in UsaState.AllValues)
        {
            object[] actionDelegates = BuildUsaStateActionDelegates(actionTryMatch.GetParameters(), out Func<string> observedAction);
            bool actionMatched = (bool)actionTryMatch.Invoke(state, actionDelegates)!;
            actionMatched.ShouldBeTrue();
            observedAction().ShouldBe(state.Value);

            object[] funcDelegates = BuildUsaStateFuncDelegates(funcMatch.GetParameters());
            string matchedValue = (string)funcMatch.Invoke(state, funcDelegates)!;
            matchedValue.ShouldBe(state.Value);
        }

        UsaState invalid = new() { Value = "XX", DisplayName = "Invalid" };
        object[] invalidDelegates = BuildUsaStateActionDelegates(actionTryMatch.GetParameters(), out _);
        bool invalidMatched = (bool)actionTryMatch.Invoke(invalid, invalidDelegates)!;
        invalidMatched.ShouldBeFalse();

        object[] invalidFuncDelegates = BuildUsaStateFuncDelegates(funcMatch.GetParameters());
        TargetInvocationException ex = Should.Throw<TargetInvocationException>(() => funcMatch.Invoke(invalid, invalidFuncDelegates));
        ex.InnerException.ShouldBeOfType<InvalidOperationException>();
    }

    private static readonly string AllowedShortCodeCharacters = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";

    private static object[] BuildUsaStateActionDelegates(ParameterInfo[] parameters, out Func<string> observedValue)
    {
        string? matched = null;
        observedValue = () => matched ?? string.Empty;

        return parameters
            .Select(parameter =>
            {
                UsaState state = ResolveUsaStateForCaseParameter(parameter.Name);
                Action action = () => matched = state.Value;
                return (object)action;
            })
            .ToArray();
    }

    private static object[] BuildUsaStateFuncDelegates(ParameterInfo[] parameters)
    {
        return parameters
            .Select(parameter =>
            {
                UsaState state = ResolveUsaStateForCaseParameter(parameter.Name);
                Func<string> func = () => state.Value;
                return (object)func;
            })
            .ToArray();
    }

    private static UsaState ResolveUsaStateForCaseParameter(string? parameterName)
    {
        parameterName.ShouldNotBeNull();
        parameterName.StartsWith("case", StringComparison.Ordinal).ShouldBeTrue();

        string stateFieldName = parameterName["case".Length..];
        FieldInfo? field = typeof(UsaState).GetField(stateFieldName, BindingFlags.Public | BindingFlags.Static);
        field.ShouldNotBeNull();
        object? value = field.GetValue(null);
        value.ShouldNotBeNull();

        return (UsaState)value;
    }
}
