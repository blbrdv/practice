using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PrimeTime;

public class DoubleConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(double);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var token = JToken.Load(reader);
        if (token.Type is JTokenType.Boolean or JTokenType.String)
        {
            throw new JsonReaderException("Number field must be a double.");
        }
        return token.ToObject<double>();
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}