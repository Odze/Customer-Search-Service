using Web.Payloads;

namespace Web.Services;

public sealed class SearchService
{
    private readonly ApiClient _api;

    public SearchService(ApiClient api)
    {
        _api = api;
    }

    public async Task<List<CustomerPayload>> SearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new List<CustomerPayload>();

        Console.WriteLine( $"search?q={Uri.EscapeDataString(query)}");
        
        string url = $"search?q={Uri.EscapeDataString(query)}";
        List<CustomerPayload>? result = await _api.GetAsync<List<CustomerPayload>>(url);
        
        return result ?? new List<CustomerPayload>();
    }
}