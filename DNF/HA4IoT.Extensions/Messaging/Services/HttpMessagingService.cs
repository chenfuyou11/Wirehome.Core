using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Messaging;
using HA4IoT.Extensions.Contracts;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace HA4IoT.Extensions.Messaging.Services
{
    public class HttpMessagingService : IHttpMessagingService
    {
        private readonly ILogger _logService;
        private readonly IMessageBrokerService _messageBroker;
        private readonly List<IMessage> _messageHandlers = new List<IMessage>();

        public HttpMessagingService(ILogService logService, IMessageBrokerService messageBroker, IEnumerable<IMessage> handlers)
        {
            _logService = logService.CreatePublisher(nameof(HttpMessagingService));
            _messageBroker = messageBroker;
            _messageHandlers.AddRange(handlers);
        }

        public void Startup()
        {
            _messageHandlers.ForEach(handler =>
            {
                _messageBroker.Subscribe(new MessageSubscription
                {
                    Id = Guid.NewGuid().ToString(),
                    PayloadType = handler.GetType().Name,
                    Topic = typeof(HttpMessagingService).Name,
                    Callback = MessageHandler
                });
            });
        }

        public void MessageHandler(Message<JObject> message)
        {
            _messageHandlers.ForEach(async handler =>
            {
                if (handler.CanSerialize(message.Payload.Type))
                {
                    try
                    {                        
                        Uri uri = new Uri(handler.MessageAddress(message.Payload.Content));
                        var webRequest = WebRequest.Create(uri);
                        webRequest.Method = "POST";
                        webRequest.ContentType = "application/x-www-form-urlencoded";
                                
                        byte[] byteArray = handler.Serialize(message.Payload.Content);
                        webRequest.Headers["ContentLength"] = byteArray.Length.ToString();

                        using (Stream stream = await webRequest.GetRequestStreamAsync())
                        {
                            stream.Write(byteArray, 0, byteArray.Length);
                        }

                        using (WebResponse response = await webRequest.GetResponseAsync())
                        {
                            using (Stream stream = response.GetResponseStream())
                            {
                                using (StreamReader sr = new StreamReader(stream))
                                {
                                    string responseStream = await sr.ReadToEndAsync();
                                }
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        _logService.Error(ex, $"Handler of type {handler.GetType().Name} failed to process message");
                    }
                }
            });
        }
    }
}
