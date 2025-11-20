using System.Collections.Concurrent;
using api.Entity;
using api.Enums;
using api.Payloads;

namespace api.Repository
{
    public sealed class CustomerRepository: IDisposable
    {
        public event Action<CustomerEntity> OnCreated;
        public event Action<CustomerEntity> OnUpdated;
        public event Action<int> OnDeleted;
        
        private readonly Dictionary<int, CustomerEntity> _customers = new();
        
        private readonly ConcurrentQueue<RepoUpdatePayload> _queue = new();
        private readonly CancellationTokenSource _cts = new();
        private readonly Task _worker;
        
        private const int DELAY_MS = 1000;
        
        public CustomerRepository()
        {
            _worker = Task.Run(WorkerLoop);
        }

        public IEnumerable<CustomerEntity> Get(IEnumerable<int> ids)
        {
            List<CustomerEntity> entities = new();
            
            foreach (int id in ids)
            {
                if (_customers.TryGetValue(id, out CustomerEntity? entity))
                    entities.Add(entity);
            }
            
            return entities;
        }
        
        public void Upsert(IEnumerable<CustomerPayload> customers)
        {
            foreach (CustomerPayload customer in customers)
            {
                Upsert(customer);
            }
        }
        
        public void Upsert(CustomerPayload payload)
        {
            _queue.Enqueue(new RepoUpdatePayload
            {
                Type = RepoUpdateType.Upsert,
                Payload = payload
            });
        }

        public void Remove(int id)
        {
            _queue.Enqueue(new RepoUpdatePayload
            {
                Type = RepoUpdateType.Delete,
                Payload = new CustomerPayload
                {
                    Id = id
                }
            });
        }
        
        private async Task WorkerLoop()
        {
            while (!_cts.IsCancellationRequested)
            {
                if (_queue.TryDequeue(out RepoUpdatePayload update))
                {
                    switch (update.Type)
                    {
                        case RepoUpdateType.Upsert:
                            if (update.Payload != null) ApplyUpsert(update.Payload.Value);
                            break;

                        case RepoUpdateType.Delete:
                            if (update.Payload != null) ApplyRemove(update.Payload.Value.Id);
                            break;
                    }
                }
                else
                {
                    await Task.Delay(DELAY_MS, _cts.Token);
                }
            }
        }
        
        private void ApplyUpsert(CustomerPayload payload)
        {
            if (_customers.TryGetValue(payload.Id, out CustomerEntity? existing))
            {
                if (existing.Patch(payload))
                    OnUpdated?.Invoke(existing);
                
                return;
            }

            CustomerEntity customer = new(payload);
            _customers[payload.Id] = customer;
            OnCreated?.Invoke(customer);
        }

        private void ApplyRemove(int id)
        {
            if (_customers.Remove(id))
                OnDeleted?.Invoke(id);
        }

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
            _worker.Dispose();
        }
    }
}