using System.ComponentModel;
using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;

namespace Incursa.Types.Tests;

public class HardeningRegressionTests
{
    [Fact]
    public void ShortCode_TryParse_RejectsInvalidAndNormalizes()
    {
        ShortCode.TryParse(null, out _).ShouldBeFalse();
        ShortCode.TryParse("abc?", out _).ShouldBeFalse();

        ShortCode.TryParse("ab-cd-ef-gh", out ShortCode parsed).ShouldBeTrue();
        parsed.RawCode.ShouldBe("ABCDEFGH");
    }

    [Fact]
    public void ShortCode_Default_IsSafe()
    {
        ShortCode code = default;

        code.IsEmpty.ShouldBeTrue();
        code.ToString().ShouldBe(string.Empty);
        code.FormattedCode.ShouldBe(string.Empty);
    }

    [Fact]
    public void Duration_RoundTripsNegativeComponents()
    {
        var parsed = Duration.Parse("P-1DT-2H");

        parsed.ToString().ShouldBe("P-1DT-2H");
    }

    [Fact]
    public void Period_EndExclusiveBoundary_IsNotContained()
    {
        var start = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var end = start.AddHours(1);
        var period = new Period(start, end);

        period.Contains(start).ShouldBeTrue();
        period.Contains(end).ShouldBeFalse();
    }

    [Fact]
    public void Period_TouchingRanges_DoNotOverlap()
    {
        var start = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var first = new Period(start, start.AddHours(1));
        var second = new Period(start.AddHours(1), start.AddHours(2));

        first.Overlaps(second).ShouldBeFalse();
    }

    [Fact]
    public void RecurringPeriod_UsesProvidedStartTime()
    {
        var recurring = RecurringPeriod.Parse("0 * * * *");
        var reference = new DateTime(2025, 1, 1, 10, 30, 0, DateTimeKind.Utc);

        Period period = recurring.GetPeriod(reference);

        period.StartInclusive.ShouldBe(new DateTimeOffset(2025, 1, 1, 10, 0, 0, TimeSpan.Zero));
        period.EndExclusive.ShouldBe(new DateTimeOffset(2025, 1, 1, 11, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public void FastId_Default_IsSafe()
    {
        FastId id = default;

        id.IsEmpty.ShouldBeTrue();
        id.Encoded.ShouldBe("0");
    }

    [Fact]
    public void JsonContext_Default_ToString_IsEmptyObject()
    {
        JsonContext context = default;

        context.ToString().ShouldBe("{}");
        context.GetData<string>("missing").ShouldBeNull();
    }

    [Fact]
    public void TypeConverters_FailFast_OnInvalidInput()
    {
        var emailConverter = TypeDescriptor.GetConverter(typeof(EmailAddress));
        var jsonConverter = TypeDescriptor.GetConverter(typeof(JsonContext));

        Should.Throw<FormatException>(() => emailConverter.ConvertFrom("not-an-email"));
        Should.Throw<FormatException>(() => jsonConverter.ConvertFrom("not-json"));
    }

    [Fact]
    public void EncryptedString_RequiresBase64Ciphertext()
    {
        EncryptedString.TryParse("plain-text", out _).ShouldBeFalse();

        var validCiphertext = Convert.ToBase64String(RandomNumberGenerator.GetBytes(24));
        var parsed = EncryptedString.Parse(validCiphertext);
        parsed.ToString().ShouldBe(validCiphertext);

        EncryptedString generated = EncryptedString.GenerateRandom();
        EncryptedString.TryParse(generated.ToString(), out _).ShouldBeTrue();
    }

    [Fact]
    public void MoneyAndPercentage_UseInvariantSerialization()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        var originalUiCulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
            CultureInfo.CurrentUICulture = new CultureInfo("fr-FR");

            string money = JsonSerializer.Serialize(new Money(12.34m));
            string percentage = JsonSerializer.Serialize(new Percentage(0.1234m));

            money.ShouldBe("\"12.34\"");
            percentage.ShouldBe("\"0.1234\"");
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }
}
