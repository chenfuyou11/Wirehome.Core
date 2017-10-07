using System;
using System.Threading.Tasks;
using System.Net.Sockets;
using Wirehome.Contracts.Logging;
using Wirehome.Extensions.Contracts;
using Wirehome.Extensions.Messaging.Core;


namespace Wirehome.Extensions.Messaging.Services
{
    public class UdpBroadcastService : ITcpMessagingService
    {
        private readonly ILogger _logService;
        private readonly IEventAggregator _eventAggregator;

        public UdpBroadcastService(ILogService logService, IEventAggregator eventAggregator)
        {
            _logService = logService.CreatePublisher(nameof(TcpMessagingService));
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
        }

        public Task Initialize()
        {
            // Add filteringto this service
            _eventAggregator.SubscribeForAsyncResult<IBinaryMessage>(MessageHandler);
            return Task.CompletedTask;
        }
        
        private Task<object> MessageHandler(IMessageEnvelope<IBinaryMessage> message)
        {
            return SendMessage(message);
        }

        private async Task<object> SendMessage(IMessageEnvelope<IBinaryMessage> message)
        {
            try
            {
                using (var socket = new UdpClient())
                {
                    var uri = new Uri($"udp://{message.Message.MessageAddress()}");

                    socket.Connect(uri.Host, uri.Port);
                    socket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 0);
                    var messageBytes = message.Message.Serialize();
                    await socket.SendAsync(messageBytes, messageBytes.Length).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"Message {message.GetType().Name} failed during send UDP broadcast message");
            }
            return null;
        }
        
    }

   
}
