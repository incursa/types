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

namespace Incursa;

[JsonConverter(typeof(CidrRangeJsonConverter))]
[TypeConverter(typeof(CidrRangeTypeConverter))]
public readonly record struct CidrRange : IParsable<CidrRange>
{
    public CidrRange(string cidr)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cidr);

        string[] parts = cidr.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
        {
            throw new ArgumentException($"Invalid CIDR expression '{cidr}'.", nameof(cidr));
        }

        IpAddress network = IpAddress.Parse(parts[0]);
        if (!int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int prefix))
        {
            throw new ArgumentException($"Invalid CIDR prefix '{parts[1]}'.", nameof(cidr));
        }

        int maxPrefix = network.IsIPv4 ? 32 : 128;
        if (prefix < 0 || prefix > maxPrefix)
        {
            throw new ArgumentOutOfRangeException(nameof(cidr), $"CIDR prefix must be between 0 and {maxPrefix}.");
        }

        Network = network;
        PrefixLength = prefix;
        Value = $"{Network}/{PrefixLength}";
    }

    public IpAddress Network { get; }

    public int PrefixLength { get; }

    public string Value { get; }

    public override string ToString() => Value;

    public bool Contains(IpAddress address)
    {
        if (address.IsIPv4 != Network.IsIPv4)
        {
            return false;
        }

        byte[] targetBytes = address.Address.GetAddressBytes();
        byte[] networkBytes = Network.Address.GetAddressBytes();

        return Mask(targetBytes, PrefixLength).SequenceEqual(Mask(networkBytes, PrefixLength));
    }

    private static byte[] Mask(byte[] bytes, int prefixLength)
    {
        if (prefixLength == 0)
        {
            return new byte[bytes.Length];
        }

        int fullBytes = prefixLength / 8;
        int remainingBits = prefixLength % 8;

        var masked = new byte[bytes.Length];
        Array.Copy(bytes, masked, fullBytes);

        if (remainingBits > 0 && fullBytes < bytes.Length)
        {
            byte mask = (byte)(0xFF << (8 - remainingBits));
            masked[fullBytes] = (byte)(bytes[fullBytes] & mask);
        }

        return masked;
    }

    public static CidrRange Parse(string s, IFormatProvider? provider) => Parse(s);

    public static CidrRange Parse(string s) => new(s);

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out CidrRange result) =>
        TryParse(s, out result);

    public static bool TryParse([NotNullWhen(true)] string? s, out CidrRange result)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            result = default;
            return false;
        }

        try
        {
            result = new CidrRange(s);
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    public class CidrRangeTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
            sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
            destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is string s && TryParse(s, out CidrRange range))
            {
                return range;
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        {
            if (value is CidrRange range && destinationType == typeof(string))
            {
                return range.ToString();
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public class CidrRangeJsonConverter : JsonConverter<CidrRange>
    {
        public override CidrRange Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? value = reader.GetString();
            if (TryParse(value, out CidrRange range))
            {
                return range;
            }

            throw new JsonException($"Invalid CIDR '{value}'.");
        }

        public override void Write(Utf8JsonWriter writer, CidrRange value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
