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

[JsonConverter(typeof(LocaleJsonConverter))]
[TypeConverter(typeof(LocaleTypeConverter))]
public readonly record struct Locale : IParsable<Locale>, ISpanFormattable
{
    public Locale(string tag)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tag);

        CultureInfo culture = CreateCulture(tag);
        Bcp47Tag = culture.Name;
        DisplayName = culture.EnglishName;
    }

    public string Bcp47Tag { get; }

    public string DisplayName { get; }

    public override string ToString() => Bcp47Tag;

    public static Locale Parse(string s, IFormatProvider? provider) => Parse(s);

    public static Locale Parse(string s)
    {
        return new Locale(s);
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out Locale result) =>
        TryParse(s, out result);

    public static bool TryParse([NotNullWhen(true)] string? s, out Locale result)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            result = default;
            return false;
        }

        try
        {
            result = new Locale(s);
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    private static CultureInfo CreateCulture(string tag)
    {
        try
        {
            return CultureInfo.GetCultureInfoByIetfLanguageTag(tag);
        }
        catch (CultureNotFoundException)
        {
            // Fallback to neutral culture parsing (e.g., en-US-x-private still resolves to en-US)
            try
            {
                return CultureInfo.GetCultureInfo(tag);
            }
            catch (CultureNotFoundException ex)
            {
                throw new ArgumentException($"Invalid BCP-47 language tag '{tag}'.", nameof(tag), ex);
            }
        }
    }

    public string ToString(string? format, IFormatProvider? formatProvider) => ToString();

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        if (destination.Length >= Bcp47Tag.Length)
        {
            Bcp47Tag.AsSpan().CopyTo(destination);
            charsWritten = Bcp47Tag.Length;
            return true;
        }

        charsWritten = 0;
        return false;
    }

    public class LocaleTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
            sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
            destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is string s && TryParse(s, out Locale locale))
            {
                return locale;
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        {
            if (value is Locale locale && destinationType == typeof(string))
            {
                return locale.ToString();
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public class LocaleJsonConverter : JsonConverter<Locale>
    {
        public override Locale Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? value = reader.GetString();
            if (TryParse(value, out Locale locale))
            {
                return locale;
            }

            throw new JsonException($"Invalid locale '{value}'.");
        }

        public override void Write(Utf8JsonWriter writer, Locale value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
