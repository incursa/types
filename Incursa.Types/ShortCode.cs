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

namespace Incursa;

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

[JsonConverter(typeof(ShortCodeJsonConverter))]
[TypeConverter(typeof(ShortCodeTypeConverter))]
public readonly record struct ShortCode : IBvParsable<ShortCode>, IParsable<ShortCode>
{
    private const int DefaultLength = 8;
    private const int MinLength = 4;
    private const int MaxLength = 32;
    private const string Chars = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";
    private readonly string? rawCode;

    public static readonly ShortCode Empty = default;

    public ShortCode()
    {
        this.rawCode = GenerateInternal(DefaultLength);
    }

    public ShortCode(string code)
    {
        this.rawCode = NormalizeAndValidate(code);
    }

    private ShortCode(string code, bool skipValidation)
    {
        this.rawCode = skipValidation ? code : NormalizeAndValidate(code);
    }

    public string FormattedCode
    {
        get
        {
            if (this.IsEmpty)
            {
                return string.Empty;
            }

            var middle = this.RawCode.Length / 2;
            return $"{this.RawCode[..middle]}-{this.RawCode[middle..]}";
        }
    }

    public string RawCode => this.rawCode ?? string.Empty;

    public bool IsEmpty => string.IsNullOrEmpty(this.rawCode);

    public static ShortCode Generate(int length)
    {
        var code = GenerateInternal(length);
        return new ShortCode(code, true);
    }

    public override string ToString() => this.RawCode;

    private static string GenerateInternal(int length)
    {
        if (length is < MinLength or > MaxLength)
        {
            throw new ArgumentOutOfRangeException(nameof(length), $"ShortCode length must be between {MinLength} and {MaxLength}.");
        }

        var charsLength = (byte)Chars.Length;

        var sb = new StringBuilder();
        for (var i = 0; i < length; i++)
        {
            byte[] randomNumber;
            do
            {
                randomNumber = RandomNumberGenerator.GetBytes(1);
            }
            while (!IsFairRoll(randomNumber[0], charsLength));

            var index = (byte)(randomNumber[0] % charsLength);
            sb.Append(Chars[index]);
        }

        return sb.ToString();
    }

    private static bool IsFairRoll(byte roll, byte maxValue)
    {
        // There are MaxValue / maxValue full sets of numbers that can come up
        // in a single byte.  For instance, if we have a 6 sided die, there are
        // 42 full sets of 1-6 that come up.  The 43rd set is incomplete.
        var fullSetsOfValues = byte.MaxValue / maxValue;

        // If the roll is within this range of fair values, then we let it continue.
        // In the 6 sided die case, a roll between 0 and 251 is allowed.  (We use
        // < rather than <= since the = portion allows through an extra 0 value).
        // 252 through 255 would provide an extra 0, 1, 2, 3 so they are not fair
        // to use.
        return roll < maxValue * fullSetsOfValues;
    }

    private static string NormalizeAndValidate(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        string normalized = code.Replace("-", string.Empty, StringComparison.Ordinal).Trim().ToUpperInvariant();
        if (normalized.Length is < MinLength or > MaxLength)
        {
            throw new ArgumentOutOfRangeException(nameof(code), $"ShortCode length must be between {MinLength} and {MaxLength}.");
        }

        foreach (char c in normalized)
        {
            if (!Chars.Contains(c))
            {
                throw new ArgumentException($"ShortCode contains an invalid character '{c}'.", nameof(code));
            }
        }

        return normalized;
    }

    public class ShortCodeJsonConverter : JsonConverter<ShortCode>
    {
        public override ShortCode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType is not JsonTokenType.String and not JsonTokenType.PropertyName)
            {
                throw new JsonException("ShortCode must be a JSON string.");
            }

            string? value = reader.GetString();
            return TryParse(value, out ShortCode parsed) ? parsed : throw new JsonException($"Invalid ShortCode '{value}'.");
        }

        public override void Write(Utf8JsonWriter writer, ShortCode value, JsonSerializerOptions options) =>
            writer.WriteStringValue(value.ToString());

        public override void WriteAsPropertyName(
            Utf8JsonWriter writer,
            ShortCode value,
            JsonSerializerOptions options) =>
                writer.WritePropertyName(value.ToString());

        public override ShortCode ReadAsPropertyName(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) =>
                this.Read(ref reader, typeToConvert, options);
    }

    // TypeConverter for ShortCode to and from string
    public class ShortCodeTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
            sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
            destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is string s)
            {
                return Parse(s);
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        {
            if (value is ShortCode type && destinationType == typeof(string))
            {
                return type.ToString();
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public static ShortCode Parse(string value)
    {
        return new ShortCode(value);
    }

    public static ShortCode Parse(string s, IFormatProvider? provider) => Parse(s);

    public static bool TryParse([NotNullWhen(true)] string? value, [MaybeNullWhen(false)] out ShortCode result)
    {
        return TryParse(value, null, out result);
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out ShortCode result)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            result = default;
            return false;
        }

        try
        {
            result = new ShortCode(s);
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }
}
