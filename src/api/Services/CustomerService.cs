using api.Payloads;

namespace api.Services
{
    public sealed class CustomerService
    {
        private readonly ApiService _apiService;

        public CustomerService(ApiService apiService)
        {
            _apiService = apiService;
        }
        
        public async Task<CustomerPayload[]> Get(int page)
        {
            try
            {
                CustomerPayload[]? response = await _apiService.GetAsync<CustomerPayload[]>($"customers?page={page}");
                return response ?? [];
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    $"Failed to fetch customers at page {page}. See inner exception for details.",
                    exception);
            }
        }
    }   
}