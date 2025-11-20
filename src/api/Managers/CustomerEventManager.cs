using api.Enums;
using api.Listeners;
using api.Payloads;
using api.Repository;

namespace api.Managers
{
    public sealed class CustomerEventManager : BackgroundService
    {
        private SocketEventListener _socketEventListener;
        
        private const string ADDRESS = "tcp://localhost:5555";
        private const string TOPIC = "dataChanges";
        
        private const string CREATE_KEY = "CREATE";
        private const string UPDATE_KEY = "UPDATE";
        private const string DELETE_KEY = "DELETE";

        private readonly CustomerRepository _customerRepository;

        public CustomerEventManager(CustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _socketEventListener = new SocketEventListener(ADDRESS, TOPIC);
            _socketEventListener.OnEvent += OnEvent;
            _socketEventListener.ExecuteAsync(stoppingToken);

            return Task.CompletedTask;
        }

        private void OnEvent(DataChangeEventPayload payload)
        {
            try
            {
                if (string.IsNullOrEmpty(payload.EventType))
                {
                    Console.Error.WriteLine("Missing EventType in payload");
                    return;
                }
                
                RepoUpdatePayload updatePayload = ConvertToRepoEvent(payload);
                HandleEvent(updatePayload);
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(exception);
            }
        }
        
        private static RepoUpdatePayload ConvertToRepoEvent(DataChangeEventPayload message)
        {
            RepoUpdateType type = message.EventType.ToUpperInvariant() switch
            {
                CREATE_KEY => RepoUpdateType.Upsert,
                UPDATE_KEY => RepoUpdateType.Upsert,
                DELETE_KEY => RepoUpdateType.Delete,
                _ => throw new Exception($"Unknown event type '{message.EventType}'")
            };

            return new RepoUpdatePayload
            {
                Type = type,
                Payload = message.Data
            };
        }

        private void HandleEvent(RepoUpdatePayload update)
        {
            CustomerPayload? payload = update.Payload;

            if (payload == null)
                throw new Exception("Payload cannot be null");

            switch (update.Type)
            {
                case RepoUpdateType.Upsert:
                    _customerRepository.Upsert(payload.Value);
                    break;

                case RepoUpdateType.Delete:
                    _customerRepository.Remove(payload.Value.Id);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void Dispose()
        {
            _socketEventListener.OnEvent -= OnEvent;
        }
    }
}