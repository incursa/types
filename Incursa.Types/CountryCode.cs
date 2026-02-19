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

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

#nullable enable

namespace Incursa;

[JsonConverter(typeof(CountryCodeJsonConverter))]
[TypeConverter(typeof(CountryCodeTypeConverter))]
public readonly record struct CountryCode : IParsable<CountryCode>, ISpanFormattable
{
    public CountryCode(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        RegionInfo region = CreateRegionInfo(code);
        TwoLetterCode = region.TwoLetterISORegionName.ToUpperInvariant();
        ThreeLetterCode = region.ThreeLetterISORegionName.ToUpperInvariant();
        EnglishName = region.EnglishName;
    }

    public string TwoLetterCode { get; }

    public string ThreeLetterCode { get; }

    public string EnglishName { get; }

    public override string ToString() => TwoLetterCode;

    public static CountryCode Parse(string s, IFormatProvider? provider) => Parse(s);

    public static CountryCode Parse(string s)
    {
        return new CountryCode(s);
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out CountryCode result) =>
        TryParse(s, out result);

    public static bool TryParse([NotNullWhen(true)] string? s, out CountryCode result)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            result = default;
            return false;
        }

        try
        {
            result = new CountryCode(s);
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        return ToString();
    }

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        if (destination.Length >= TwoLetterCode.Length)
        {
            TwoLetterCode.AsSpan().CopyTo(destination);
            charsWritten = TwoLetterCode.Length;
            return true;
        }

        charsWritten = 0;
        return false;
    }

    private static RegionInfo CreateRegionInfo(string value)
    {
        string trimmed = value.Trim();
        if (trimmed.Length is 2 or 3)
        {
            string upper = trimmed.ToUpperInvariant();
            if (upper.Length == 3)
            {
                if (ThreeToTwoLookup.Value.TryGetValue(upper, out string? alpha2))
                {
                    return new RegionInfo(alpha2);
                }

                throw new ArgumentException($"Invalid country code '{value}'.", nameof(value));
            }

            return new RegionInfo(upper);
        }

        // Try to resolve from cultures when more detail is provided (e.g., en-US)
        try
        {
            CultureInfo culture = CultureInfo.GetCultureInfo(trimmed);
            return new RegionInfo(culture.Name);
        }
        catch (CultureNotFoundException ex)
        {
            throw new ArgumentException($"Invalid country code '{value}'.", nameof(value), ex);
        }
    }

    private static readonly Lazy<Dictionary<string, string>> ThreeToTwoLookup = new(() =>
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (CultureInfo culture in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
        {
            try
            {
                RegionInfo region = new(culture.Name);
                map[region.ThreeLetterISORegionName] = region.TwoLetterISORegionName;
            }
            catch
            {
                // Ignore cultures that cannot produce a RegionInfo
            }
        }

        return map;
    });

    public class CountryCodeTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
            sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
            destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is string s && TryParse(s, out CountryCode code))
            {
                return code;
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        {
            if (value is CountryCode country && destinationType == typeof(string))
            {
                return country.ToString();
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public class CountryCodeJsonConverter : JsonConverter<CountryCode>
    {
        public override CountryCode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? value = reader.GetString();
            if (TryParse(value, out CountryCode code))
            {
                return code;
            }

            throw new JsonException($"Invalid country code '{value}'.");
        }

        public override void Write(Utf8JsonWriter writer, CountryCode value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
