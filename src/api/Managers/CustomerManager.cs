using api.Entity;
using api.Index;
using api.Payloads;
using api.Repository;
using api.Services;

namespace api.Managers
{
    public sealed class CustomerManager : BackgroundService
    {
        private readonly CustomerService _customerService;
        private readonly CustomerRepository _customerRepository;
        private readonly CustomerLuceneIndex _customerLuceneIndex;
        
        public CustomerManager(
            CustomerService customerService,
            CustomerRepository customerRepository,
            CustomerLuceneIndex customerLuceneIndex
        )
        {
            _customerService = customerService;
            _customerRepository = customerRepository;
            _customerLuceneIndex = customerLuceneIndex;

            _customerRepository.OnCreated += IndexCustomer;
            _customerRepository.OnUpdated += IndexCustomer;
            _customerRepository.OnDeleted += DeleteCustomer;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Fetch(stoppingToken);
        }

        private async Task Fetch(CancellationToken cancellationToken)
        {
            int page = 1;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    CustomerPayload[] customers = await _customerService.Get(page);

                    if (customers.Length == 0)
                        break;

                    _customerRepository.Upsert(customers);
                    page++;
                }
                catch (Exception exception)
                {
                    Console.Error.WriteLine(exception);
                }
            }
        }

        private void IndexCustomer(CustomerEntity entity)
        {
            _customerLuceneIndex.Index(entity);
        }

        private void DeleteCustomer(int id)
        {
            _customerLuceneIndex.Delete(id);
        }
        
        public override void Dispose()
        {
            _customerRepository.OnCreated -= IndexCustomer;
            _customerRepository.OnUpdated -= IndexCustomer;
            _customerRepository.OnDeleted -= DeleteCustomer;
        }
    }
}