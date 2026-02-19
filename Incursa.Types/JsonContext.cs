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

using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

[JsonConverter(typeof(JsonContextJsonConverter))]
[TypeConverter(typeof(JsonContextTypeConverter))]
public readonly record struct JsonContext
{
    private readonly JsonObject? rawData;

    public JsonContext(JsonObject value)
    {
        this.rawData = value ?? throw new ArgumentNullException(nameof(value));
    }

    public JsonContext(IReadOnlyDictionary<string, JsonValue> values)
    {
        var json = new JsonObject();
        foreach (var value in values)
        {
            json[value.Key] = value.Value;
        }

        this.rawData = json;
    }

    public JsonContext(IReadOnlyDictionary<string, string> values)
    {
        var json = new JsonObject();
        foreach (var value in values)
        {
            json[value.Key] = JsonValue.Create(value.Value);
        }

        this.rawData = json;
    }

    public JsonObject RawData => this.rawData ?? new JsonObject();

    public string Value => this.ToString();

    public override string ToString() => this.RawData.ToJsonString();

    public T GetData<T>(JsonTypeInfo<T> jsonTypeInfo)
    {
        return this.RawData.Deserialize(jsonTypeInfo);
    }

    public T GetData<T>(string name, JsonTypeInfo<T> jsonTypeInfo)
    {
        JsonNode? prop = this.RawData?[name];
        return prop is null ? default : prop.Deserialize(jsonTypeInfo);
    }

    public T GetData<T>()
    {
        return this.RawData.Deserialize<T>();
    }

    public T GetData<T>(string name)
    {
        JsonNode? prop = this.RawData?[name];
        return prop is null ? default : prop.Deserialize<T>();
    }

    public void SetData<T>(string name, T data, JsonTypeInfo<T> jsonTypeInfo)
        where T : class
    {
        JsonObject target = this.EnsureWritableRawData();
        target[name] = JsonNode.Parse(JsonSerializer.Serialize(data, jsonTypeInfo));
    }

    public void SetData(string name, object data, JsonTypeInfo jsonTypeInfo)
    {
        JsonObject target = this.EnsureWritableRawData();
        target[name] = JsonNode.Parse(JsonSerializer.Serialize(data, jsonTypeInfo));
    }

    public void SetData<T>(string name, T data)
        where T : class
    {
        JsonObject target = this.EnsureWritableRawData();
        target[name] = JsonNode.Parse(JsonSerializer.Serialize<T>(data));
    }

    public void SetData(string name, object data)
    {
        JsonObject target = this.EnsureWritableRawData();
        target[name] = JsonNode.Parse(JsonSerializer.Serialize(data));
    }

    public static JsonContext Empty() => new(new JsonObject());

    // public static JsonContext FromObject(object data, JsonTypeInfo jsonTypeInfo) => new(JsonSerializer.SerializeToNode(data, jsonTypeInfo).AsObject());

    public static JsonContext FromObject<T>(T data, JsonTypeInfo<T> jsonTypeInfo)
    {
        if (data is null)
        {
            return Empty();
        }

        var serialized = JsonSerializer.SerializeToNode(data, jsonTypeInfo);
        if (serialized is null)
        {
            return Empty();
        }

        return new(serialized.AsObject());
    }

    public static JsonContext FromObject<T>(T data)
    {
        var serialized = JsonSerializer.SerializeToNode(data);
        if (serialized is null)
        {
            return Empty();
        }

        return new(serialized.AsObject());
    }

    public static JsonContext? FromJson(JsonObject? data) => data is null ? null : new JsonContext(data);

    public static JsonContext? TryParse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        try
        {
            JsonObject? jobj = JsonNode.Parse(value)?.AsObject();
            return jobj is null ? null : new JsonContext(jobj);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static bool TryParse(string value, out JsonContext id)
    {
        JsonContext? temp = TryParse(value);
        if (temp is not null)
        {
            id = temp.Value;
            return true;
        }

        id = default;
        return false;
    }

    public static JsonContext Parse(string value) => TryParse(value) ?? throw new FormatException($"Invalid JSON object value '{value}'.");

    private JsonObject EnsureWritableRawData() =>
        this.rawData ?? throw new InvalidOperationException("Cannot mutate a default JsonContext. Use JsonContext.Empty().");

    public class JsonContextJsonConverter : JsonConverter<JsonContext>
    {
        public override JsonContext Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            JsonObject? jobj = JsonObject.Parse(ref reader)?.AsObject();
            if (jobj is not null)
            {
                return new JsonContext(jobj);
            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, JsonContext value, JsonSerializerOptions options)
        {
            value.RawData.WriteTo(writer);
        }
    }

    // TypeConverter for JsonContext to and from string
    public class JsonContextTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
            sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
            destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is string s)
            {
                return JsonContext.TryParse(s) ?? throw new FormatException($"Invalid JsonContext value '{s}'.");
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType) =>
            value is JsonContext type && destinationType == typeof(string)
                ? type.ToString()
                : base.ConvertTo(context, culture, value, destinationType);
    }
}
