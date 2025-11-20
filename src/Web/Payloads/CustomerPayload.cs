using Newtonsoft.Json;

namespace Web.Payloads
{
    public struct CustomerPayload
    {
        [JsonProperty( PropertyName = "id")]
        public int Id { get; init; }
        
        [JsonProperty( PropertyName = "firstName")]
        public string? FirstName { get; init; }
        
        [JsonProperty( PropertyName = "lastName")]
        public string? LastName { get; init; }
        
        [JsonProperty( PropertyName = "email")]
        public string[]? Email { get; init; }
    }
}