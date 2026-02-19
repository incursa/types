using System.ComponentModel;
using System.Text.Json;

namespace Incursa.Types.Tests;

public class SpecDrivenSupportingTypesTests
{
    [Fact]
    public void MonthOnly_Parse_And_ToString_AreCanonical()
    {
        var value = MonthOnly.Parse("2025-07");

        Assert.Equal(2025, value.Year);
        Assert.Equal(7, value.Month);
        Assert.Equal("2025-07", value.ToString());
    }

    [Fact]
    public void MonthOnly_TryParse_Invalid_ReturnsFalse()
    {
        Assert.False(MonthOnly.TryParse("invalid", out var parsed));
        Assert.Equal(default, parsed);
    }

    [Fact]
    public void MonthOnly_AddMonths_Overflow_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MonthOnly.MaxValue.AddMonths(1));
    }

    [Fact]
    public void MonthOnly_TypeConverter_RoundTripsString()
    {
        var converter = TypeDescriptor.GetConverter(typeof(MonthOnly));
        var parsed = Assert.IsType<MonthOnly>(converter.ConvertFrom("2025-07"));

        Assert.Equal("2025-07", converter.ConvertTo(parsed, typeof(string)));
    }

    [Fact]
    public void MonthOnly_JsonConverter_RoundTrips()
    {
        var value = new MonthOnly(2024, 12);
        var json = JsonSerializer.Serialize(value);

        Assert.Equal("\"2024-12\"", json);
        Assert.Equal(value, JsonSerializer.Deserialize<MonthOnly>(json));
    }

    [Fact]
    public void JsonContext_Empty_SetData_And_GetData_RoundTrip()
    {
        var context = JsonContext.Empty();
        context.SetData("name", "sam");
        context.SetData("age", 42);

        Assert.Equal("sam", context.GetData<string>("name"));
        Assert.Equal(42, context.GetData<int>("age"));
    }

    [Fact]
    public void JsonContext_Default_SetData_Throws()
    {
        JsonContext context = default;

        Assert.Throws<InvalidOperationException>(() => context.SetData("x", "y"));
    }

    [Fact]
    public void JsonContext_TryParse_Invalid_ReturnsNullAndFalse()
    {
        Assert.Null(JsonContext.TryParse("{not-valid-json}"));
        Assert.False(JsonContext.TryParse("{not-valid-json}", out var parsed));
        Assert.Equal(default, parsed);
    }

    [Fact]
    public void JsonContext_TypeConverter_RoundTripsString()
    {
        var converter = TypeDescriptor.GetConverter(typeof(JsonContext));
        var parsed = Assert.IsType<JsonContext>(converter.ConvertFrom("{\"k\":\"v\"}"));

        Assert.Equal("v", parsed.GetData<string>("k"));
        Assert.Equal("{\"k\":\"v\"}", converter.ConvertTo(parsed, typeof(string)));
    }

    [Fact]
    public void JsonContext_FromObject_Null_ReturnsEmpty()
    {
        var context = JsonContext.FromObject<string?>(null);

        Assert.Equal("{}", context.ToString());
    }

    [Fact]
    public void Locale_TryParse_Invalid_ReturnsFalse()
    {
        Assert.False(Locale.TryParse("invalid-tag-$$", out var locale));
        Assert.Equal(default, locale);
    }

    [Fact]
    public void Locale_TryFormat_ReturnsFalse_WhenDestinationTooSmall()
    {
        var locale = Locale.Parse("en-US");
        Span<char> buffer = stackalloc char[2];

        var ok = locale.TryFormat(buffer, out var written, default, null);

        Assert.False(ok);
        Assert.Equal(0, written);
    }

    [Fact]
    public void Locale_TypeConverter_RoundTripsString()
    {
        var converter = TypeDescriptor.GetConverter(typeof(Locale));
        var parsed = Assert.IsType<Locale>(converter.ConvertFrom("fr-FR"));

        Assert.Equal("fr-FR", converter.ConvertTo(parsed, typeof(string)));
    }

    [Fact]
    public void Locale_JsonConverter_Invalid_Throws()
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Locale>("\"invalid-tag-$$\""));
    }

    [Fact]
    public void MoneyExtensions_Sum_WorksForEnumerableAndSelector()
    {
        var values = new[] { new Money(1.11m), new Money(2.22m), new Money(3.33m) };
        var selectorSum = values.Sum(v => v);
        var directSum = values.Sum();

        Assert.Equal(new Money(6.66m), selectorSum);
        Assert.Equal(new Money(6.66m), directSum);
    }

    [Fact]
    public void MoneyExtensions_Sum_NullableSource_UsesZeroForNulls()
    {
        Money?[] values = [new Money(1.10m), null, new Money(2.20m)];

        var sum = values.Sum();

        Assert.Equal(new Money(3.30m), sum);
    }

    [Fact]
    public void VirtualPathExtensions_FileOperations_AreConsistent()
    {
        var path = new VirtualPath("assets/images/logo.png");

        Assert.Equal("logo.png", path.GetFileName());
        Assert.Equal("logo", path.GetFileNameWithoutExtension());
        Assert.Equal(".png", path.GetExtension());
        Assert.True(path.HasExtension());
    }

    [Fact]
    public void VirtualPathExtensions_ChangeExtension_UpdatesPath()
    {
        var path = new VirtualPath("assets/images/logo.png");

        var updated = path.ChangeExtension(".jpg");

        Assert.Equal("assets/images/logo.jpg", updated.ToString());
    }

    [Fact]
    public void VirtualPathExtensions_Combine_EmptyString_ReturnsOriginal()
    {
        var path = new VirtualPath("assets/images");

        var combined = path.Combine(string.Empty);

        Assert.Equal(path, combined);
    }

    [Fact]
    public void CidrRange_PrefixZero_ContainsAnyAddressInFamily()
    {
        var cidr = CidrRange.Parse("0.0.0.0/0");

        Assert.True(cidr.Contains(IpAddress.Parse("10.1.2.3")));
        Assert.False(cidr.Contains(IpAddress.Parse("2001:db8::1")));
    }

    [Fact]
    public void TimeZoneId_TryParse_Invalid_ReturnsFalse()
    {
        Assert.False(TimeZoneId.TryParse("Not/A_Real_Time_Zone", out var tz));
        Assert.Equal(default, tz);
    }

    [Fact]
    public void Url_TryParse_RelativeUrl_IsNotAbsolute()
    {
        Assert.True(Url.TryParse("/assets/logo.png", out var url));
        Assert.False(url.IsAbsolute);
    }

    [Fact]
    public void Maybe_None_Value_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => _ = Maybe<int>.None.Value);
    }

    [Fact]
    public void Maybe_TryGetValue_None_ReturnsFalse()
    {
        var ok = Maybe<int>.None.TryGetValue(out var value);

        Assert.False(ok);
        Assert.Equal(default, value);
    }

    [Fact]
    public void BvFile_FromBase64_DefaultOverload_UsesDefaults()
    {
        var bytes = new byte[] { 1, 2, 3 };
        var base64 = Convert.ToBase64String(bytes);

        var file = BvFile.FromBase64(base64);

        Assert.Equal("file", file.FileName);
        Assert.Equal("application/octet-stream", file.ContentType);
        Assert.Equal(bytes, file.Data.ToArray());
    }
}
