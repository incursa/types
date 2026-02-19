using System.ComponentModel;
using System.Globalization;
using System.Text.Json;

namespace Incursa.Types.Tests;

public class SpecDrivenWaveOneHardeningTests
{
    [Fact]
    public void JsonContext_Default_ReadsAsEmptyObject_ButCannotMutate()
    {
        JsonContext context = default;

        context.RawData.Count.ShouldBe(0);
        context.ToString().ShouldBe("{}");
        context.GetData<string>("missing").ShouldBeNull();
        Should.Throw<InvalidOperationException>(() => context.SetData("name", "sam"));
    }

    [Fact]
    public void JsonContext_Parse_TryParse_And_JsonConverter_FollowObjectContract()
    {
        JsonContext.TryParse("{\"name\":\"sam\",\"age\":42}", out JsonContext parsed).ShouldBeTrue();
        parsed.GetData<string>("name").ShouldBe("sam");
        parsed.GetData<int>("age").ShouldBe(42);

        JsonContext.TryParse("[1,2,3]").ShouldBeNull();
        JsonContext.TryParse("{not-json}", out _).ShouldBeFalse();
        Should.Throw<FormatException>(() => JsonContext.Parse("[1,2,3]"));

        string json = JsonSerializer.Serialize(parsed);
        JsonContext roundTrip = JsonSerializer.Deserialize<JsonContext>(json);
        roundTrip.GetData<string>("name").ShouldBe("sam");
        roundTrip.GetData<int>("age").ShouldBe(42);
    }

    [Fact]
    public void JsonContext_TypeConverter_RejectsInvalid()
    {
        TypeConverter converter = TypeDescriptor.GetConverter(typeof(JsonContext));
        Should.Throw<FormatException>(() => converter.ConvertFrom("invalid-json"));
    }

    [Fact]
    public void MonthOnly_Boundaries_And_Arithmetic_AreDeterministic()
    {
        MonthOnly min = MonthOnly.FromMonthNumber(0);
        MonthOnly max = MonthOnly.FromMonthNumber(119_987);

        min.ToString().ShouldBe("0001-01");
        max.ToString().ShouldBe("9999-12");

        Should.Throw<ArgumentOutOfRangeException>(() => MonthOnly.FromMonthNumber(-1));
        Should.Throw<ArgumentOutOfRangeException>(() => MonthOnly.FromMonthNumber(119_988));
        Should.Throw<ArgumentOutOfRangeException>(() => MonthOnly.MaxValue.AddMonths(1));
        Should.Throw<ArgumentOutOfRangeException>(() => MonthOnly.MinValue.AddMonths(-1));
    }

    [Fact]
    public void MonthOnly_Parse_And_Converters_UseCanonicalYearMonth()
    {
        MonthOnly.Parse("24-1").ToString().ShouldBe("0024-01");
        MonthOnly.Parse("2025/07/15").ToString().ShouldBe("2025-07");
        MonthOnly.TryParse("invalid", out _).ShouldBeFalse();
        Should.Throw<FormatException>(() => MonthOnly.Parse("invalid"));

        TypeConverter converter = TypeDescriptor.GetConverter(typeof(MonthOnly));
        MonthOnly fromConverter = (MonthOnly)converter.ConvertFrom("2024-12")!;
        converter.ConvertTo(fromConverter, typeof(string)).ShouldBe("2024-12");

        string json = JsonSerializer.Serialize(new MonthOnly(2024, 12));
        JsonSerializer.Deserialize<MonthOnly>(json).ToString().ShouldBe("2024-12");
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<MonthOnly>("123"));
    }

    [Fact]
    public void MonthOnly_CompareTo_ObjectContract_IsStrict()
    {
        MonthOnly january = new(2024, 1);
        january.CompareTo(null).ShouldBe(1);
        Should.Throw<ArgumentException>(() => january.CompareTo(new object()));
    }

    [Fact]
    public void BvFile_Constructors_And_Factories_PreserveMetadataAndData()
    {
        byte[] bytes = [1, 2, 3, 4];
        string base64 = Convert.ToBase64String(bytes);

        BvFile fromBase64 = BvFile.FromBase64(base64, "data.bin", "application/octet-stream");
        fromBase64.FileName.ShouldBe("data.bin");
        fromBase64.ContentType.ShouldBe("application/octet-stream");
        fromBase64.Data.ToArray().ShouldBe(bytes);

        BvFile defaults = BvFile.FromBase64(base64);
        defaults.FileName.ShouldBe("file");
        defaults.ContentType.ShouldBe("application/octet-stream");
        defaults.Data.ToArray().ShouldBe(bytes);
    }

    [Fact]
    public void BvFile_PathFactories_ValidateInputs_AndResolveMimeTypes()
    {
        string tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.txt");
        try
        {
            File.WriteAllText(tempPath, "hello world");
            BvFile fromPath = BvFile.FromPath(tempPath);
            fromPath.FileName.ShouldBe(Path.GetFileName(tempPath));
            fromPath.ContentType.ShouldBe("text/plain");
            fromPath.Data.ToArray().Length.ShouldBeGreaterThan(0);

            BvFile fromInfo = BvFile.FromFileInfo(new FileInfo(tempPath));
            fromInfo.FileName.ShouldBe(fromPath.FileName);
            fromInfo.ContentType.ShouldBe(fromPath.ContentType);
            fromInfo.Data.ToArray().ShouldBe(fromPath.Data.ToArray());
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }

        Should.Throw<FormatException>(() => BvFile.FromBase64("not-base64", "x.bin", "application/octet-stream"));
        Should.Throw<ArgumentException>(() => BvFile.FromPath(" "));
        Should.Throw<ArgumentNullException>(() => BvFile.FromFileInfo(null!));
    }

    [Fact]
    public void BvFile_JsonConstructor_RoundTrips()
    {
        BvFile source = BvFile.FromBase64(Convert.ToBase64String([9, 8, 7]), "payload.bin", "application/octet-stream");

        string json = JsonSerializer.Serialize(source);
        BvFile roundTrip = JsonSerializer.Deserialize<BvFile>(json)!;

        roundTrip.FileName.ShouldBe(source.FileName);
        roundTrip.ContentType.ShouldBe(source.ContentType);
        roundTrip.Data.ToArray().ShouldBe(source.Data.ToArray());
    }

    [Fact]
    public void EmailAddress_ParseTryParse_And_Converters_UseNormalizedValue()
    {
        EmailAddress.TryParse("User@Exámple.com", out EmailAddress parsed).ShouldBeTrue();
        parsed.Value.ShouldBe("user@xn--exmple-qta.com");

        EmailAddress.TryParse("not-an-email", out _).ShouldBeFalse();
        Assert.ThrowsAny<Exception>(() => EmailAddress.Parse("not-an-email"));

        TypeConverter converter = TypeDescriptor.GetConverter(typeof(EmailAddress));
        EmailAddress converted = (EmailAddress)converter.ConvertFrom("Jane <Jane.Doe@Example.com>")!;
        converter.ConvertTo(converted, typeof(string)).ShouldBe("jane.doe@example.com");
        Should.Throw<FormatException>(() => converter.ConvertFrom("bad-email"));

        JsonSerializer.Deserialize<EmailAddress>("\"user@example.com\"").Value.ShouldBe("user@example.com");
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<EmailAddress>("\"bad-email\""));
    }

    [Fact]
    public void EmailAddress_GenerateRandom_ProducesParseableCanonicalEmail()
    {
        EmailAddress generated = EmailAddress.GenerateRandom();

        generated.Value.ShouldContain("@example.test");
        EmailAddress.TryParse(generated.Value, out _).ShouldBeTrue();
    }

    [Fact]
    public void EncryptedString_Validation_RejectsMalformedCiphertext()
    {
        EncryptedString.TryParse("plain-text", out _).ShouldBeFalse();
        EncryptedString.TryParse("abc", out _).ShouldBeFalse();
        EncryptedString.TryParse("AAAAAAAAAAAAAAA=", out _).ShouldBeFalse(); // decodes too short

        Should.Throw<FormatException>(() => EncryptedString.Parse("abc"));
        Should.Throw<FormatException>(() => EncryptedString.Parse("AAAAAAAAAAAAAAA="));
    }

    [Fact]
    public void EncryptedString_Converters_And_Generation_RoundTrip()
    {
        EncryptedString generated = EncryptedString.GenerateRandom();
        EncryptedString.TryParse(generated.Value, out _).ShouldBeTrue();

        TypeConverter converter = TypeDescriptor.GetConverter(typeof(EncryptedString));
        EncryptedString fromConverter = (EncryptedString)converter.ConvertFrom(generated.Value)!;
        converter.ConvertTo(fromConverter, typeof(string)).ShouldBe(generated.Value);
        Should.Throw<FormatException>(() => converter.ConvertFrom("not-base64"));

        JsonSerializer.Deserialize<EncryptedString>($"\"{generated.Value}\"").Value.ShouldBe(generated.Value);
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<EncryptedString>("\"not-base64\""));
    }

    [Fact]
    public void TimeZoneId_Parse_And_Converters_ResolveCanonicalIdentifier()
    {
        TimeZoneId windows = TimeZoneId.Parse(" Pacific Standard Time ");
        windows.CanonicalId.ShouldBe("America/Los_Angeles");
        windows.ToString().ShouldBe("America/Los_Angeles");

        TimeZoneId iana = TimeZoneId.Parse("America/New_York");
        iana.CanonicalId.ShouldBe("America/New_York");

        TimeZoneId.TryParse("Not/A_Zone", out _).ShouldBeFalse();
        Should.Throw<ArgumentException>(() => TimeZoneId.Parse("Not/A_Zone"));

        TypeConverter converter = TypeDescriptor.GetConverter(typeof(TimeZoneId));
        TimeZoneId converted = (TimeZoneId)converter.ConvertFrom("Pacific Standard Time")!;
        converter.ConvertTo(converted, typeof(string)).ShouldBe("America/Los_Angeles");

        JsonSerializer.Deserialize<TimeZoneId>("\"Pacific Standard Time\"").CanonicalId.ShouldBe("America/Los_Angeles");
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<TimeZoneId>("\"Not/A_Zone\""));
    }

    [Fact]
    public void Url_Normalization_And_Converters_AreDeterministic()
    {
        Url absolute = Url.Parse(" HTTPS://ExAmple.com:443 ");
        absolute.IsAbsolute.ShouldBeTrue();
        absolute.Value.ShouldBe("https://example.com/");

        Url withQuery = Url.Parse("http://Example.com:80/api?x=1#frag");
        withQuery.Value.ShouldBe("http://example.com/api?x=1#frag");

        Url relative = Url.Parse(" /assets/logo.png ");
        relative.IsAbsolute.ShouldBeFalse();
        relative.Value.ShouldBe("/assets/logo.png");

        Url.TryParse("http://example.com", out _).ShouldBeTrue();
        Url.TryParse("http://[::1", out _).ShouldBeFalse();
        Should.Throw<ArgumentException>(() => Url.Parse("http://[::1"));

        TypeConverter converter = TypeDescriptor.GetConverter(typeof(Url));
        Url converted = (Url)converter.ConvertFrom("HTTP://Example.com:80")!;
        converter.ConvertTo(converted, typeof(string)).ShouldBe("http://example.com/");
        Should.Throw<NotSupportedException>(() => converter.ConvertFrom("http://[::1"));

        JsonSerializer.Deserialize<Url>("\"http://example.com\"").Value.ShouldBe("http://example.com/");
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<Url>("\"http://[::1\""));
    }

    [Fact]
    public void Percentage_Formatting_Parsing_Converters_AndComparison_AreConsistent()
    {
        Percentage pct = new(0.12349m);
        pct.Value.ShouldBe(0.1234m);
        pct.ScaledValue.ShouldBe(12.34m);
        pct.ToString().ShouldBe("12.34%");
        pct.ToStringRaw().ShouldBe("12.34%");
        pct.ToString(1).ShouldBe("12.3%");

        Percentage.Parse("0.125").Value.ShouldBe(0.125m);
        Percentage.ParseScaled("12.5").Value.ShouldBe(0.125m);
        Percentage.TryParseScaled("x", out _).ShouldBeFalse();

        Percentage.TryParse("0,5", new CultureInfo("fr-FR"), out Percentage parsedWithProvider).ShouldBeTrue();
        parsedWithProvider.Value.ShouldBe(0.5m);
        Percentage.TryParse("0,5", CultureInfo.InvariantCulture, out Percentage parsedInvariant).ShouldBeTrue();
        parsedInvariant.Value.ShouldBe(5m);

        TypeConverter converter = TypeDescriptor.GetConverter(typeof(Percentage));
        Percentage converted = (Percentage)converter.ConvertFrom(0.1234m)!;
        converter.ConvertTo(converted, typeof(decimal)).ShouldBe(0.1234m);
        converter.ConvertTo(converted, typeof(string)).ShouldBe("12.34");

        JsonSerializer.Deserialize<Percentage>("0.12349").Value.ShouldBe(0.1234m);
        JsonSerializer.Deserialize<Percentage>("\"0.12349\"").Value.ShouldBe(0.1234m);
        Should.Throw<ArgumentException>(() => pct.CompareTo("not-a-percentage"));
    }

    [Fact]
    public void MoneyExtensions_Sum_HandlesEmptyAndNullableSelectors()
    {
        Array.Empty<Money>().Sum().ShouldBe(Money.Zero);
        Array.Empty<Money?>().Sum().ShouldBe(Money.Zero);

        var entries = new[] { 10m, 20m, 30m };
        entries.Sum(v => new Money(v)).ShouldBe(new Money(60m));

        var maybeEntries = new decimal?[] { 10m, null, 20m };
        Money? maybeSum = maybeEntries.Sum(v => v.HasValue ? new Money(v.Value) : (Money?)null);
        maybeSum.ShouldNotBeNull();
        maybeSum.Value.ShouldBe(new Money(30m));

        Money? allNull = new decimal?[] { null, null }.Sum(v => v.HasValue ? new Money(v.Value) : (Money?)null);
        allNull.ShouldNotBeNull();
        allNull.Value.ShouldBe(Money.Zero);
    }
}
