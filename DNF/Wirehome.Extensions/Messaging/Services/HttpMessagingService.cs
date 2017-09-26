using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Wirehome.Contracts.Logging;
using Wirehome.Extensions.Contracts;
using Wirehome.Extensions.Messaging.Core;

namespace Wirehome.Extensions.Messaging.Services
{
    public class HttpMessagingService : IHttpMessagingService
    {
        private readonly ILogger _logService;
        private readonly IEventAggregator _eventAggregator;

        public HttpMessagingService(ILogService logService, IEventAggregator eventAggregator)
        {
            _logService = logService.CreatePublisher(nameof(HttpMessagingService));
            _eventAggregator = eventAggregator;
        }

        //Add Dispose - maybe to all IService
        public void Startup()
        {
            _eventAggregator.SubscribeForAsyncResult<IHttpMessage>(MessageHandler);
        }

        public async Task<object> MessageHandler(IMessageEnvelope<IHttpMessage> message)
        {
            var httpMessage = message.Message;

            if (httpMessage.RequestType == "POST")
            {
                return await HandlePostRequest(httpMessage).ConfigureAwait(false);
            }
            else
            if (httpMessage.RequestType == "GET")
            {
                return await HandleGetRequest(httpMessage).ConfigureAwait(false);
            }

            return null;
        }

        private async Task<object> HandleGetRequest(IHttpMessage httpMessage)
        {
            using (var httpClient = new HttpClient())
            {
                var address = httpMessage.MessageAddress();
                var httpResponse = await httpClient.GetAsync(address).ConfigureAwait(false);
                httpResponse.EnsureSuccessStatusCode();
                var responseBody = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

                return httpMessage.ParseResult(responseBody);
            }
        }

        private static async Task<object> HandlePostRequest(IHttpMessage httpMessage)
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

                return httpMessage.ParseResult(responseBody);
            }
        }
    }
}
