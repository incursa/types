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

[JsonConverter(typeof(UrlJsonConverter))]
[TypeConverter(typeof(UrlTypeConverter))]
public readonly record struct Url : IParsable<Url>
{
    public Url(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        if (!Uri.TryCreate(value.Trim(), UriKind.RelativeOrAbsolute, out Uri? parsed))
        {
            throw new ArgumentException($"Invalid URL '{value}'.", nameof(value));
        }

        IsAbsolute = parsed.IsAbsoluteUri;
        Uri = Normalize(parsed);
        Value = Uri.ToString();
    }

    public string Value { get; }

    public Uri Uri { get; }

    public bool IsAbsolute { get; }

    public override string ToString() => Value;

    public static Url Parse(string s, IFormatProvider? provider) => Parse(s);

    public static Url Parse(string s)
    {
        return new Url(s);
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out Url result) =>
        TryParse(s, out result);

    public static bool TryParse([NotNullWhen(true)] string? s, out Url result)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            result = default;
            return false;
        }

        try
        {
            result = new Url(s);
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    private static Uri Normalize(Uri uri)
    {
        if (!uri.IsAbsoluteUri)
        {
            return uri; // nothing to normalize for relative references
        }

        var builder = new UriBuilder(uri)
        {
            Host = uri.IdnHost.ToLowerInvariant(),
            Scheme = uri.Scheme.ToLowerInvariant(),
        };

        // Remove default ports and ensure path canonicalization
        if ((string.Equals(builder.Scheme, Uri.UriSchemeHttp, StringComparison.Ordinal) && builder.Port == 80) || (string.Equals(builder.Scheme, Uri.UriSchemeHttps, StringComparison.Ordinal) && builder.Port == 443))
        {
            builder.Port = -1;
        }

        string path = string.IsNullOrEmpty(builder.Path) ? "/" : builder.Path;
        builder.Path = path;

        return builder.Uri;
    }

    public class UrlTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
            sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
            destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is string s && TryParse(s, out Url url))
            {
                return url;
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        {
            if (value is Url url && destinationType == typeof(string))
            {
                return url.ToString();
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public class UrlJsonConverter : JsonConverter<Url>
    {
        public override Url Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? value = reader.GetString();
            if (TryParse(value, out Url url))
            {
                return url;
            }

            throw new JsonException($"Invalid url '{value}'.");
        }

        public override void Write(Utf8JsonWriter writer, Url value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
