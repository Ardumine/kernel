using System.Numerics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AFCP.JsonConverters;


//https://www.answeroverflow.com/m/1239304152430153849

public class Vector2JsonConverter : JsonConverter<Vector2>
{
    public override Vector2 Read(ref Utf8JsonReader reader, Type typeInfo, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected StartObject");

        float x = 0, y = 0;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                return new Vector2(x, y);

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                string propertyName = reader.GetString()!;
                reader.Read();

                switch (propertyName)
                {
                    case "X":
                        x = reader.GetSingle();
                        break;
                    case "Y":
                        y = reader.GetSingle();
                        break;
                }
            }
        }

        throw new JsonException("Could not deserialize Vector2");
    }

    public override void Write(Utf8JsonWriter writer, Vector2 value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("X", value.X);
        writer.WriteNumber("Y", value.Y);
        writer.WriteEndObject();
    }
}


// Custom JSON converter for PieceOfData with type preservation
public class Vector3JsonConverter : JsonConverter<Vector3>
{
    public override Vector3 Read(ref Utf8JsonReader reader, Type typeInfo, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected StartObject");

        float x = 0, y = 0, z = 0;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                return new Vector3(x, y, z);

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                string propertyName = reader.GetString()!;
                reader.Read();

                switch (propertyName)
                {
                    case "X":
                        x = reader.GetSingle();
                        break;
                    case "Y":
                        y = reader.GetSingle();
                        break;
                    case "Z":
                        z = reader.GetSingle();
                        break;
                }
            }
        }

        throw new JsonException("Could not deserialize Vector3");
    }

    public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("X", value.X);
        writer.WriteNumber("Y", value.Y);
        writer.WriteNumber("Z", value.Z);
        writer.WriteEndObject();
    }
}



public class ObjectToInferredTypesConverter : JsonConverter<object>
{
    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Deserialize based on the current JSON token type
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                // Check if the string is a base64 encoded byte array
                string stringValue = reader.GetString()!;
                try
                {
                    byte[] byteArray = Convert.FromBase64String(stringValue);
                    return byteArray;
                }
                catch
                {
                    return stringValue;
                }
            case JsonTokenType.Number:
                if (reader.TryGetInt32(out int intValue))
                    return intValue;
                return reader.GetDouble();
            case JsonTokenType.True:
            case JsonTokenType.False:
                return reader.GetBoolean();
            case JsonTokenType.StartObject:
                // For complex objects, use the default deserializer
                return JsonSerializer.Deserialize(ref reader, typeof(object), options);
            case JsonTokenType.StartArray:
                // For arrays, use the default deserializer
                return JsonSerializer.Deserialize(ref reader, typeof(object), options);

            default:
                return null;
        }
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        // Special handling for byte arrays
        if (value is byte[] byteArray)
        {
            writer.WriteStringValue(Convert.ToBase64String(byteArray));
            return;
        }

        // Use the default serializer for writing
        JsonSerializer.Serialize(writer, value, value?.GetType() ?? typeof(object), options);
    }
}

//https://stackoverflow.com/questions/76263968/c-sharp-parse-byte-to-json-without-converting-to-base64-string
public class ByteArrayConverter : JsonConverter<byte[]>
{
    public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, value.AsEnumerable(), options);

    public override byte[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType switch
        {
            JsonTokenType.String => reader.GetBytesFromBase64(),
            JsonTokenType.StartArray => JsonSerializer.Deserialize<List<byte>>(ref reader, options)!.ToArray(),
            JsonTokenType.Null => null,
            _ => throw new JsonException(),
        };
}