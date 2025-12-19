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

namespace Bravellian;

[JsonConverter(typeof(CurrencyCodeJsonConverter))]
[TypeConverter(typeof(CurrencyCodeTypeConverter))]
public readonly record struct CurrencyCode : IParsable<CurrencyCode>, ISpanFormattable
{
    public CurrencyCode(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        string normalized = code.Trim().ToUpperInvariant();
        if (!Iso4217CurrencyData.Metadata.TryGetValue(normalized, out CurrencyMetadata metadata))
        {
            throw new ArgumentException($"Invalid ISO 4217 currency code '{code}'.", nameof(code));
        }

        Code = normalized;
        NumericCode = metadata.NumericCode;
        MinorUnit = metadata.MinorUnit;
    }

    public string Code { get; }

    public int NumericCode { get; }

    public int MinorUnit { get; }

    public override string ToString() => Code;

    public string ToString(string? format, IFormatProvider? formatProvider) => Code;

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        if (destination.Length >= Code.Length)
        {
            Code.AsSpan().CopyTo(destination);
            charsWritten = Code.Length;
            return true;
        }

        charsWritten = 0;
        return false;
    }

    public static CurrencyCode Parse(string s, IFormatProvider? provider) => Parse(s);

    public static CurrencyCode Parse(string s)
    {
        return new CurrencyCode(s);
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out CurrencyCode result) =>
        TryParse(s, out result);

    public static bool TryParse([NotNullWhen(true)] string? s, out CurrencyCode result)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            result = default;
            return false;
        }

        try
        {
            result = new CurrencyCode(s);
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    public class CurrencyCodeTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
            sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
            destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is string s && TryParse(s, out CurrencyCode code))
            {
                return code;
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        {
            if (value is CurrencyCode code && destinationType == typeof(string))
            {
                return code.ToString();
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public class CurrencyCodeJsonConverter : JsonConverter<CurrencyCode>
    {
        public override CurrencyCode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? value = reader.GetString();
            if (TryParse(value, out CurrencyCode code))
            {
                return code;
            }

            throw new JsonException($"Invalid currency code '{value}'.");
        }

        public override void Write(Utf8JsonWriter writer, CurrencyCode value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}

internal readonly record struct CurrencyMetadata(int NumericCode, int MinorUnit);

internal static class Iso4217CurrencyData
{
    public static readonly IReadOnlyDictionary<string, CurrencyMetadata> Metadata = new Dictionary<string, CurrencyMetadata>(StringComparer.OrdinalIgnoreCase)
    {
            {"AED", new CurrencyMetadata(784, 2)},
            {"AFN", new CurrencyMetadata(971, 2)},
            {"ALL", new CurrencyMetadata(8, 2)},
            {"AMD", new CurrencyMetadata(51, 2)},
            {"AOA", new CurrencyMetadata(973, 2)},
            {"ARS", new CurrencyMetadata(32, 2)},
            {"AUD", new CurrencyMetadata(36, 2)},
            {"AWG", new CurrencyMetadata(533, 2)},
            {"AZN", new CurrencyMetadata(944, 2)},
            {"BAM", new CurrencyMetadata(977, 2)},
            {"BBD", new CurrencyMetadata(52, 2)},
            {"BDT", new CurrencyMetadata(50, 2)},
            {"BGN", new CurrencyMetadata(975, 2)},
            {"BHD", new CurrencyMetadata(48, 3)},
            {"BIF", new CurrencyMetadata(108, 0)},
            {"BMD", new CurrencyMetadata(60, 2)},
            {"BND", new CurrencyMetadata(96, 2)},
            {"BOB", new CurrencyMetadata(68, 2)},
            {"BOV", new CurrencyMetadata(984, 2)},
            {"BRL", new CurrencyMetadata(986, 2)},
            {"BSD", new CurrencyMetadata(44, 2)},
            {"BTN", new CurrencyMetadata(64, 2)},
            {"BWP", new CurrencyMetadata(72, 2)},
            {"BYN", new CurrencyMetadata(933, 2)},
            {"BZD", new CurrencyMetadata(84, 2)},
            {"CAD", new CurrencyMetadata(124, 2)},
            {"CDF", new CurrencyMetadata(976, 2)},
            {"CHE", new CurrencyMetadata(947, 2)},
            {"CHF", new CurrencyMetadata(756, 2)},
            {"CHW", new CurrencyMetadata(948, 2)},
            {"CLF", new CurrencyMetadata(990, 4)},
            {"CLP", new CurrencyMetadata(152, 0)},
            {"CNY", new CurrencyMetadata(156, 2)},
            {"COP", new CurrencyMetadata(170, 2)},
            {"COU", new CurrencyMetadata(970, 2)},
            {"CRC", new CurrencyMetadata(188, 2)},
            {"CUP", new CurrencyMetadata(192, 2)},
            {"CVE", new CurrencyMetadata(132, 2)},
            {"CZK", new CurrencyMetadata(203, 2)},
            {"DJF", new CurrencyMetadata(262, 0)},
            {"DKK", new CurrencyMetadata(208, 2)},
            {"DOP", new CurrencyMetadata(214, 2)},
            {"DZD", new CurrencyMetadata(12, 2)},
            {"EGP", new CurrencyMetadata(818, 2)},
            {"ERN", new CurrencyMetadata(232, 2)},
            {"ETB", new CurrencyMetadata(230, 2)},
            {"EUR", new CurrencyMetadata(978, 2)},
            {"FJD", new CurrencyMetadata(242, 2)},
            {"FKP", new CurrencyMetadata(238, 2)},
            {"GBP", new CurrencyMetadata(826, 2)},
            {"GEL", new CurrencyMetadata(981, 2)},
            {"GHS", new CurrencyMetadata(936, 2)},
            {"GIP", new CurrencyMetadata(292, 2)},
            {"GMD", new CurrencyMetadata(270, 2)},
            {"GNF", new CurrencyMetadata(324, 0)},
            {"GTQ", new CurrencyMetadata(320, 2)},
            {"GYD", new CurrencyMetadata(328, 2)},
            {"HKD", new CurrencyMetadata(344, 2)},
            {"HNL", new CurrencyMetadata(340, 2)},
            {"HTG", new CurrencyMetadata(332, 2)},
            {"HUF", new CurrencyMetadata(348, 2)},
            {"IDR", new CurrencyMetadata(360, 2)},
            {"ILS", new CurrencyMetadata(376, 2)},
            {"INR", new CurrencyMetadata(356, 2)},
            {"IQD", new CurrencyMetadata(368, 3)},
            {"IRR", new CurrencyMetadata(364, 2)},
            {"ISK", new CurrencyMetadata(352, 0)},
            {"JMD", new CurrencyMetadata(388, 2)},
            {"JOD", new CurrencyMetadata(400, 3)},
            {"JPY", new CurrencyMetadata(392, 0)},
            {"KES", new CurrencyMetadata(404, 2)},
            {"KGS", new CurrencyMetadata(417, 2)},
            {"KHR", new CurrencyMetadata(116, 2)},
            {"KMF", new CurrencyMetadata(174, 0)},
            {"KPW", new CurrencyMetadata(408, 2)},
            {"KRW", new CurrencyMetadata(410, 0)},
            {"KWD", new CurrencyMetadata(414, 3)},
            {"KYD", new CurrencyMetadata(136, 2)},
            {"KZT", new CurrencyMetadata(398, 2)},
            {"LAK", new CurrencyMetadata(418, 2)},
            {"LBP", new CurrencyMetadata(422, 2)},
            {"LKR", new CurrencyMetadata(144, 2)},
            {"LRD", new CurrencyMetadata(430, 2)},
            {"LSL", new CurrencyMetadata(426, 2)},
            {"LYD", new CurrencyMetadata(434, 3)},
            {"MAD", new CurrencyMetadata(504, 2)},
            {"MDL", new CurrencyMetadata(498, 2)},
            {"MGA", new CurrencyMetadata(969, 2)},
            {"MKD", new CurrencyMetadata(807, 2)},
            {"MMK", new CurrencyMetadata(104, 2)},
            {"MNT", new CurrencyMetadata(496, 2)},
            {"MOP", new CurrencyMetadata(446, 2)},
            {"MRU", new CurrencyMetadata(929, 2)},
            {"MUR", new CurrencyMetadata(480, 2)},
            {"MVR", new CurrencyMetadata(462, 2)},
            {"MWK", new CurrencyMetadata(454, 2)},
            {"MXN", new CurrencyMetadata(484, 2)},
            {"MXV", new CurrencyMetadata(979, 2)},
            {"MYR", new CurrencyMetadata(458, 2)},
            {"MZN", new CurrencyMetadata(943, 2)},
            {"NAD", new CurrencyMetadata(516, 2)},
            {"NGN", new CurrencyMetadata(566, 2)},
            {"NIO", new CurrencyMetadata(558, 2)},
            {"NOK", new CurrencyMetadata(578, 2)},
            {"NPR", new CurrencyMetadata(524, 2)},
            {"NZD", new CurrencyMetadata(554, 2)},
            {"OMR", new CurrencyMetadata(512, 3)},
            {"PAB", new CurrencyMetadata(590, 2)},
            {"PEN", new CurrencyMetadata(604, 2)},
            {"PGK", new CurrencyMetadata(598, 2)},
            {"PHP", new CurrencyMetadata(608, 2)},
            {"PKR", new CurrencyMetadata(586, 2)},
            {"PLN", new CurrencyMetadata(985, 2)},
            {"PYG", new CurrencyMetadata(600, 0)},
            {"QAR", new CurrencyMetadata(634, 2)},
            {"RON", new CurrencyMetadata(946, 2)},
            {"RSD", new CurrencyMetadata(941, 2)},
            {"RUB", new CurrencyMetadata(643, 2)},
            {"RWF", new CurrencyMetadata(646, 0)},
            {"SAR", new CurrencyMetadata(682, 2)},
            {"SBD", new CurrencyMetadata(90, 2)},
            {"SCR", new CurrencyMetadata(690, 2)},
            {"SDG", new CurrencyMetadata(938, 2)},
            {"SEK", new CurrencyMetadata(752, 2)},
            {"SGD", new CurrencyMetadata(702, 2)},
            {"SHP", new CurrencyMetadata(654, 2)},
            {"SLE", new CurrencyMetadata(925, 2)},
            {"SOS", new CurrencyMetadata(706, 2)},
            {"SRD", new CurrencyMetadata(968, 2)},
            {"SSP", new CurrencyMetadata(728, 2)},
            {"STN", new CurrencyMetadata(930, 2)},
            {"SVC", new CurrencyMetadata(222, 2)},
            {"SYP", new CurrencyMetadata(760, 2)},
            {"SZL", new CurrencyMetadata(748, 2)},
            {"THB", new CurrencyMetadata(764, 2)},
            {"TJS", new CurrencyMetadata(972, 2)},
            {"TMT", new CurrencyMetadata(934, 2)},
            {"TND", new CurrencyMetadata(788, 3)},
            {"TOP", new CurrencyMetadata(776, 2)},
            {"TRY", new CurrencyMetadata(949, 2)},
            {"TTD", new CurrencyMetadata(780, 2)},
            {"TWD", new CurrencyMetadata(901, 2)},
            {"TZS", new CurrencyMetadata(834, 2)},
            {"UAH", new CurrencyMetadata(980, 2)},
            {"UGX", new CurrencyMetadata(800, 0)},
            {"USD", new CurrencyMetadata(840, 2)},
            {"USN", new CurrencyMetadata(997, 2)},
            {"UYI", new CurrencyMetadata(940, 0)},
            {"UYU", new CurrencyMetadata(858, 2)},
            {"UYW", new CurrencyMetadata(927, 4)},
            {"UZS", new CurrencyMetadata(860, 2)},
            {"VED", new CurrencyMetadata(926, 2)},
            {"VES", new CurrencyMetadata(928, 2)},
            {"VND", new CurrencyMetadata(704, 0)},
            {"VUV", new CurrencyMetadata(548, 0)},
            {"WST", new CurrencyMetadata(882, 2)},
            {"XAD", new CurrencyMetadata(396, 2)},
            {"XAF", new CurrencyMetadata(950, 0)},
            {"XAG", new CurrencyMetadata(961, 2)},
            {"XAU", new CurrencyMetadata(959, 2)},
            {"XBA", new CurrencyMetadata(955, 2)},
            {"XBB", new CurrencyMetadata(956, 2)},
            {"XBC", new CurrencyMetadata(957, 2)},
            {"XBD", new CurrencyMetadata(958, 2)},
            {"XCD", new CurrencyMetadata(951, 2)},
            {"XCG", new CurrencyMetadata(532, 2)},
            {"XDR", new CurrencyMetadata(960, 2)},
            {"XOF", new CurrencyMetadata(952, 0)},
            {"XPD", new CurrencyMetadata(964, 2)},
            {"XPF", new CurrencyMetadata(953, 0)},
            {"XPT", new CurrencyMetadata(962, 2)},
            {"XSU", new CurrencyMetadata(994, 2)},
            {"XTS", new CurrencyMetadata(963, 2)},
            {"XUA", new CurrencyMetadata(965, 2)},
            {"XXX", new CurrencyMetadata(999, 2)},
            {"YER", new CurrencyMetadata(886, 2)},
            {"ZAR", new CurrencyMetadata(710, 2)},
            {"ZMW", new CurrencyMetadata(967, 2)},
            {"ZWG", new CurrencyMetadata(924, 2)},
    };
}
