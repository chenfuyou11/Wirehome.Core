using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Messaging;
using HA4IoT.Extensions.Contracts;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace HA4IoT.Extensions.Messaging.Services
{
    public class HttpMessagingService : IHttpMessagingService
    {
        private readonly ILogger _logService;
        private readonly IMessageBrokerService _messageBroker;
        private readonly List<IBinaryMessage> _messageHandlers = new List<IBinaryMessage>();

        public HttpMessagingService(ILogService logService, IMessageBrokerService messageBroker, IEnumerable<IBinaryMessage> handlers)
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

        public async void MessageHandler(Message<JObject> message)
        {
            var tasks = _messageHandlers.Select(i => HandleMessage(message, i));
            await Task.WhenAll(tasks);
        }

        private async Task HandleMessage(Message<JObject> message, IBinaryMessage handler)
        {
            if (handler.CanSerialize(message.Payload.Type))
            {
                try
                {
                    var httpMessage = message.Payload.Content.ToObject<HttpMessage>();

                    if (httpMessage.RequestType == "POST")
                    {
                        await HandlePostRequest(httpMessage);
                    }
                    else if (httpMessage.RequestType == "GET")
                    {
                        await HandleGetRequest(httpMessage);
                    }
                }
                catch (Exception ex)
                {
                    _logService.Error(ex, $"Handler of type {handler.GetType().Name} failed to process message");
                }
            }
        }

        private static async Task HandleGetRequest(HttpMessage httpMessage)
        {
            using (var httpClient = new HttpClient())
            {
                var httpResponse = await httpClient.GetAsync(httpMessage.MessageAddress());
                httpResponse.EnsureSuccessStatusCode();
                var responseBody = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                httpMessage.ValidateResponse(responseBody);
            }
        }

        private static async Task HandlePostRequest(HttpMessage httpMessage)
        {
            var httpClientHandler = new HttpClientHandler();
            if (httpMessage.Cookies != null)
            {
                httpClientHandler.CookieContainer = httpMessage.Cookies;
                httpClientHandler.UseCookies = true;
            }

            if (httpMessage.Creditionals != null)
            {
                httpClientHandler.Credentials = httpMessage.Creditionals;
            }
            
            using (var httpClient = new HttpClient(httpClientHandler))
            {
                foreach (var header in httpMessage.DefaultHeaders)
                {
                    httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                }

                if (!string.IsNullOrWhiteSpace(httpMessage.AuthorisationHeader.Key))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(httpMessage.AuthorisationHeader.Key, httpMessage.AuthorisationHeader.Value);
                }

                var content = new StringContent(httpMessage.Serialize());
                if (!string.IsNullOrWhiteSpace(httpMessage.ContentType))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue(httpMessage.ContentType);
                }
                var response = await httpClient.PostAsync(httpMessage.MessageAddress(), content).ConfigureAwait(false);
                var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                httpMessage.ValidateResponse(responseBody);
            }
        }
    }
}
