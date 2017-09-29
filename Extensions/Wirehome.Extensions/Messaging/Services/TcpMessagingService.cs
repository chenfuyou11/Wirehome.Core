using Wirehome.Contracts.Logging;
using Wirehome.Contracts.Messaging;
using Wirehome.Extensions.Contracts;
using Wirehome.Extensions.Messaging.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wirehome.Contracts.Core;

namespace Wirehome.Extensions.Messaging.Services
{
    public class TcpMessagingService : ITcpMessagingService
    {
        private readonly ILogger _logService;
        private readonly IMessageBrokerService _messageBroker;
        private readonly List<IBinaryMessage> _messageHandlers = new List<IBinaryMessage>();
        private readonly INativeTCPSocketFactory _nativeTCPSocketFactory;

        private const int TIMEOUT = 500;

        public TcpMessagingService(ILogService logService, IMessageBrokerService messageBroker, IEnumerable<IBinaryMessage> handlers, INativeTCPSocketFactory nativeTCPSocketFactory)
        {
            _logService = logService.CreatePublisher(nameof(TcpMessagingService));
            _messageBroker = messageBroker;
            _messageHandlers.AddRange(handlers);
            _nativeTCPSocketFactory = nativeTCPSocketFactory ?? throw new ArgumentNullException(nameof(nativeTCPSocketFactory));
        }

        public Task Initialize()
        {
            _messageHandlers.ForEach(handler =>
            {
                _messageBroker.Subscribe(new MessageSubscription
                {
                    Id = Guid.NewGuid().ToString(),
                    PayloadType = handler.GetType().Name,
                    Topic = typeof(TcpMessagingService).Name,
                    Callback = MessageHandler
                });
            });

            return Task.CompletedTask;
        }

        private async void MessageHandler(Message<JObject> message)
        {
            var tasks = _messageHandlers.Select(i => SendMessage(message, i));
            await Task.WhenAll(tasks);
        }

        private async Task SendMessage(Message<JObject> message, IBinaryMessage handler)
        {
            if (handler.CanSerialize(message.Payload.Type))
            {
                try
                {
                    var tcpMessage = message.Payload.Content.ToObject<IBaseMessage>();

                    using (var socket = _nativeTCPSocketFactory.Create())
                    {
                        var uri = new Uri($"tcp://{tcpMessage.Address}");

                        await socket.ConnectAsync(uri.Host, uri.Port, TIMEOUT).ConfigureAwait(false);
                        var messageBytes = handler.Serialize(message.Payload.Content);
                        await socket.SendDataAsync(messageBytes, TIMEOUT, true).ConfigureAwait(false);
                        var response = await socket.ReadLineAsync().ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    _logService.Error(ex, $"Handler of type {handler.GetType().Name} failed to process message");
                }
            }
        }
    }

   
}
