namespace Web.Services
{
    public sealed class ApiClient(HttpClient httpClient)
    {
        public async Task<T?> GetAsync<T>(string url)
        {
            return await httpClient.GetFromJsonAsync<T>(url);
        }
    }
}