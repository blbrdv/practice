using Newtonsoft.Json;

namespace PrimeTime;

public class Payload
{
    [JsonProperty("method")]
    public string Method { get; set; }
    
    [JsonProperty("number")]
    public double? Number { get; set; }
}