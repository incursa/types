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
using System.Net;

#nullable enable

namespace Bravellian;

[JsonConverter(typeof(IpAddressJsonConverter))]
[TypeConverter(typeof(IpAddressTypeConverter))]
public readonly record struct IpAddress : IParsable<IpAddress>
{
    public IpAddress(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        if (!IPAddress.TryParse(value.Trim(), out IPAddress? parsed))
        {
            throw new ArgumentException($"Invalid IP address '{value}'.", nameof(value));
        }

        Address = parsed;
        Value = parsed.ToString();
    }

    public string Value { get; }

    public IPAddress Address { get; }

    public bool IsIPv4 => Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork;

    public bool IsIPv6 => Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6;

    public override string ToString() => Value;

    public static IpAddress Parse(string s, IFormatProvider? provider) => Parse(s);

    public static IpAddress Parse(string s) => new(s);

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out IpAddress result) =>
        TryParse(s, out result);

    public static bool TryParse([NotNullWhen(true)] string? s, out IpAddress result)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            result = default;
            return false;
        }

        try
        {
            result = new IpAddress(s);
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    public class IpAddressTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
            sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
            destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is string s && TryParse(s, out IpAddress ip))
            {
                return ip;
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        {
            if (value is IpAddress ip && destinationType == typeof(string))
            {
                return ip.ToString();
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public class IpAddressJsonConverter : JsonConverter<IpAddress>
    {
        public override IpAddress Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? value = reader.GetString();
            if (TryParse(value, out IpAddress ip))
            {
                return ip;
            }

            throw new JsonException($"Invalid IP address '{value}'.");
        }

        public override void Write(Utf8JsonWriter writer, IpAddress value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
