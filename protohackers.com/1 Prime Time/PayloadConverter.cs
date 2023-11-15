using Newtonsoft.Json;

namespace PrimeTime;

public class PayloadConverter : JsonConverter<Payload>
{
    public override Payload ReadJson(JsonReader reader, Type objectType, Payload existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        dynamic payload = new Payload();
        while (reader.Read())
        {
            if (reader.TokenType == JsonToken.PropertyName)
            {
                var propertyName = reader.Value.ToString();
                reader.Read();
                switch (propertyName)
                {
                    case "method":
                        payload.Method = reader.Value.ToString();
                        break;
                    case "number":
                        if (reader.Value is bool)
                        {
                            throw new JsonSerializationException("Number cannot be bool");
                        }
                        
                        if (reader.Value is long longValue)
                        {
                            payload.Number = longValue;
                        }
                        else
                        {
                            throw new JsonSerializationException($"Unexpected value for 'number': {reader.Value}");
                        }
                        break;
                    default:
                        throw new JsonSerializationException($"Unexpected property: {propertyName}");
                }
            }
            else if (reader.TokenType == JsonToken.EndObject)
            {
                break;
            }
        }
        if (payload.Method == null)
        {
            throw new JsonSerializationException("Missing 'method' property");
        }
        if (payload.Number == null)
        {
            throw new JsonSerializationException("Missing or invalid 'number' property");
        }
        return payload;
    }

    public override void WriteJson(JsonWriter writer, Payload value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
    
    public override bool CanWrite => false;
}