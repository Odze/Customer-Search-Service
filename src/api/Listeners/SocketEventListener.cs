using api.Payloads;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;

namespace api.Listeners
{
    public sealed class SocketEventListener
    {
        public event Action<DataChangeEventPayload> OnEvent;
        
        private readonly string _address;
        private readonly string _topic;
        
        public SocketEventListener(string address, string topic)
        {
            _address = address;
            _topic = topic;
        }
        
        public void ExecuteAsync(CancellationToken stoppingToken)
        {
            Task.Run(() =>
            {
                using SubscriberSocket sub = new();
                
                sub.Connect(_address);
                sub.Subscribe(_topic);

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        // First frame is the topic; we must read it or the next read won't get the JSON.
                        string topic = sub.ReceiveFrameString();
                        string json = sub.ReceiveFrameString();

                        DataChangeEventPayload simEvent = JsonConvert.DeserializeObject<DataChangeEventPayload>(json);
                        OnEvent?.Invoke(simEvent);
                    }
                    catch (Exception exception)
                    {
                        Console.Error.WriteLine($"[Topic:{_topic}] Error: {exception}");
                    }
                }
            }, stoppingToken);
        }
    }
}
