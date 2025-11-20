using Newtonsoft.Json;

namespace api.Payloads
{
    public readonly struct DataChangeEventPayload
    {
        [JsonProperty(PropertyName = "eventType")]
        public string EventType { get; init; }

        [JsonProperty(PropertyName = "data")] public CustomerPayload? Data { get; init; }
    }
}