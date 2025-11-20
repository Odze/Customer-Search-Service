using api.Entity;
using api.Extensions;
using api.Index;
using api.Payloads;
using api.Repository;

namespace api.Services
{
    public sealed class SearchService
    {
        private readonly CustomerLuceneIndex _customerLuceneIndex;
        private readonly CustomerRepository _customerRepository;

        public SearchService(CustomerLuceneIndex customerLuceneIndex, CustomerRepository customerRepository)
        {
            _customerLuceneIndex = customerLuceneIndex;
            _customerRepository = customerRepository;
        }
        
        public IEnumerable<CustomerPayload> Search(string query)
        {
            IReadOnlyList<int> ids = _customerLuceneIndex.Search(query);
            IEnumerable<CustomerEntity> costumers = _customerRepository.Get(ids);

            return costumers.ToPayloads();
        }
    }
}