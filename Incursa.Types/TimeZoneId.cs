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
using System.Security;

#nullable enable

namespace Incursa;

[JsonConverter(typeof(TimeZoneIdJsonConverter))]
[TypeConverter(typeof(TimeZoneIdTypeConverter))]
public readonly record struct TimeZoneId : IParsable<TimeZoneId>
{
    public TimeZoneId(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        string trimmed = id.Trim();

        TimeZoneInfo info = ResolveTimeZone(trimmed);
        CanonicalId = NormalizeIdentifier(trimmed, info);
        TimeZoneInfo = info;
    }

    public string CanonicalId { get; }

    public TimeZoneInfo TimeZoneInfo { get; }

    public override string ToString() => CanonicalId;

    public static TimeZoneId Parse(string s, IFormatProvider? provider) => Parse(s);

    public static TimeZoneId Parse(string s)
    {
        return new TimeZoneId(s);
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out TimeZoneId result) =>
        TryParse(s, out result);

    public static bool TryParse([NotNullWhen(true)] string? s, out TimeZoneId result)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            result = default;
            return false;
        }

        try
        {
            result = new TimeZoneId(s);
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    private static TimeZoneInfo ResolveTimeZone(string id)
    {
        try
        {
            return ResolveSystemTimeZone(id);
        }
        catch (TimeZoneNotFoundException ex) when (RequiresUnavailableIanaData(id))
        {
            throw CreatePlatformDataException(id, ex);
        }
        catch (TimeZoneNotFoundException ex)
        {
            throw new ArgumentException($"Invalid time zone identifier '{id}'.", nameof(id), ex);
        }
        catch (InvalidTimeZoneException ex)
        {
            throw new InvalidTimeZoneException($"System time zone data for '{id}' is invalid.", ex);
        }
        catch (SecurityException ex)
        {
            throw new PlatformNotSupportedException($"System time zone data for '{id}' is not accessible on this host.", ex);
        }
    }

    private static TimeZoneInfo ResolveSystemTimeZone(string id)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(id);
        }
        catch (TimeZoneNotFoundException) when (TryGetCanonicalIanaAlias(id, out string? canonicalIanaId))
        {
            return TimeZoneInfo.FindSystemTimeZoneById(canonicalIanaId);
        }
    }

    private static bool TryGetCanonicalIanaAlias([NotNullWhen(true)] string? id, [NotNullWhen(true)] out string? canonicalIanaId)
    {
        canonicalIanaId = null;
        return id is not null &&
               id.Contains('/', StringComparison.Ordinal) &&
               TimeZoneInfo.TryConvertIanaIdToWindowsId(id, out string? windowsId) &&
               TimeZoneInfo.TryConvertWindowsIdToIanaId(windowsId, out canonicalIanaId) &&
               !string.Equals(id, canonicalIanaId, StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeIdentifier(string originalId, TimeZoneInfo info)
    {
        if (TimeZoneInfo.TryConvertWindowsIdToIanaId(originalId, out string? ianaId))
        {
            return ianaId;
        }

        if (info.HasIanaId)
        {
            return originalId;
        }

        if (TimeZoneInfo.TryConvertWindowsIdToIanaId(info.Id, out ianaId))
        {
            return ianaId;
        }

        throw CreatePlatformDataException(originalId);
    }

    private static bool RequiresUnavailableIanaData(string id) =>
        !HasIanaConversionData() &&
        (id.Contains('/', StringComparison.Ordinal) ||
         id.Contains(' ', StringComparison.Ordinal) ||
         string.Equals(id, "UTC", StringComparison.OrdinalIgnoreCase));

    private static bool HasIanaConversionData() =>
        TimeZoneInfo.TryConvertWindowsIdToIanaId("UTC", out _);

    private static PlatformNotSupportedException CreatePlatformDataException(string id, Exception? innerException = null) =>
        new(
            $"Time zone identifier '{id}' requires ICU/CLDR conversion data that is unavailable on this host. " +
            "Enable ICU globalization data and disable invariant or Windows NLS-only globalization modes.",
            innerException);

    public class TimeZoneIdTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
            sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
            destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

        public override object? ConvertFrom(ITypeDescriptorContext? context, System.Globalization.CultureInfo? culture, object value)
        {
            if (value is string s && TryParse(s, out TimeZoneId id))
            {
                return id;
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object? ConvertTo(ITypeDescriptorContext? context, System.Globalization.CultureInfo? culture, object? value, Type destinationType)
        {
            if (value is TimeZoneId id && destinationType == typeof(string))
            {
                return id.ToString();
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public class TimeZoneIdJsonConverter : JsonConverter<TimeZoneId>
    {
        public override TimeZoneId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? value = reader.GetString();
            if (TryParse(value, out TimeZoneId tz))
            {
                return tz;
            }

            throw new JsonException($"Invalid time zone '{value}'.");
        }

        public override void Write(Utf8JsonWriter writer, TimeZoneId value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
