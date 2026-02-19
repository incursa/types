using System.Text.Json;

namespace Incursa.Types.Tests;

public class NewValueObjectsTests
{
    [Fact]
    public void EmailAddress_Normalizes_Domain_And_Local()
    {
        var email = EmailAddress.Parse("User@Exámple.com");

        email.Value.ShouldBe("user@xn--exmple-qta.com");
        email.Address.Host.ShouldBe("xn--exmple-qta.com");
    }

    [Fact]
    public void EmailAddress_Allows_DisplayName()
    {
        var email = EmailAddress.Parse("Jane Doe <jane.doe@example.com>");

        email.Value.ShouldBe("jane.doe@example.com");
        email.Address.DisplayName.ShouldBe("Jane Doe");
    }

    [Theory]
    [InlineData("+1 415 555 2671", "+14155552671", "US")]
    [InlineData("+44 20 7031 3000", "+442070313000", "GB")]
    public void PhoneNumber_Formats_To_E164(string raw, string expected, string region)
    {
        var number = PhoneNumber.Parse(raw);

        number.Number.ShouldBe(expected);
        number.RegionCode.ShouldBe(region);
    }

    [Theory]
    [InlineData("999")] // emergency style short-code
    [InlineData("12345")]
    public void PhoneNumber_Rejects_ShortCodes(string value)
    {
        Should.Throw<FormatException>(() => PhoneNumber.Parse(value));
    }

    [Fact]
    public void CountryCode_Parses_Two_And_Three_Letter_Forms()
    {
        var fromTwo = CountryCode.Parse("us");
        var fromThree = CountryCode.Parse("usa");

        fromTwo.TwoLetterCode.ShouldBe("US");
        fromTwo.ThreeLetterCode.ShouldBe("USA");
        fromThree.TwoLetterCode.ShouldBe("US");
    }

    [Fact]
    public void CurrencyCode_Provides_Metadata()
    {
        var usd = CurrencyCode.Parse("usd");

        usd.Code.ShouldBe("USD");
        usd.NumericCode.ShouldBe(840);
        usd.MinorUnit.ShouldBe(2);
    }

    [Fact]
    public void TimeZoneId_Bridges_Windows_And_Iana()
    {
        var tz = TimeZoneId.Parse("Pacific Standard Time");

        tz.CanonicalId.ShouldBe("America/Los_Angeles");
        tz.TimeZoneInfo.DisplayName.ShouldContain("Pacific");
    }

    [Fact]
    public void Url_Normalizes_Host_And_Default_Port()
    {
        var url = Url.Parse("HTTP://Example.com:80/api");

        url.Value.ShouldBe("http://example.com/api");
        url.IsAbsolute.ShouldBeTrue();
    }

    [Fact]
    public void IpAddress_Parses_V4_And_V6()
    {
        var v4 = IpAddress.Parse("192.168.0.1");
        var v6 = IpAddress.Parse("2001:db8::1");

        v4.IsIPv4.ShouldBeTrue();
        v6.IsIPv6.ShouldBeTrue();
    }

    [Fact]
    public void CidrRange_Checks_Containment()
    {
        var cidr = CidrRange.Parse("192.168.0.0/16");

        cidr.Contains(IpAddress.Parse("192.168.1.5")).ShouldBeTrue();
        cidr.Contains(IpAddress.Parse("10.0.0.1")).ShouldBeFalse();
    }

    [Fact]
    public void Locale_Normalizes_Bcp47()
    {
        var locale = Locale.Parse("en-us");

        locale.Bcp47Tag.ShouldBe("en-US");
        locale.DisplayName.ShouldContain("English");
    }

    [Fact]
    public void New_Types_Serialize_And_Deserialize()
    {
        var payload = new
        {
            Email = EmailAddress.Parse("user@example.com"),
            Phone = PhoneNumber.Parse("+14155552671"),
            Country = CountryCode.Parse("US"),
            Currency = CurrencyCode.Parse("USD"),
            TimeZone = TimeZoneId.Parse("America/New_York"),
            Locale = Locale.Parse("fr-FR"),
            Url = Url.Parse("https://example.com/foo"),
            Ip = IpAddress.Parse("127.0.0.1"),
            Cidr = CidrRange.Parse("127.0.0.0/8"),
        };

        string json = JsonSerializer.Serialize(payload);
        var roundtrip = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json)!;

        roundtrip["Email"].GetString().ShouldBe("user@example.com");
        roundtrip["Phone"].GetString().ShouldBe("+14155552671");
        roundtrip["Country"].GetString().ShouldBe("US");
        roundtrip["Currency"].GetString().ShouldBe("USD");
        roundtrip["TimeZone"].GetString().ShouldBe(TimeZoneId.Parse("America/New_York").ToString());
        roundtrip["Locale"].GetString().ShouldBe("fr-FR");
        roundtrip["Url"].GetString().ShouldBe("https://example.com/foo");
        roundtrip["Ip"].GetString().ShouldBe("127.0.0.1");
        roundtrip["Cidr"].GetString().ShouldBe("127.0.0.0/8");
    }
}
