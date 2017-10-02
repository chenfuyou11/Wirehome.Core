using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Wirehome.Extensions.Contracts;
using Wirehome.Extensions.Messaging.Core;
using Wirehome.Extensions.Extensions;
using System.Text;

namespace Wirehome.Extensions.Messaging.Services
{
    public class HttpMessagingService : IHttpMessagingService
    {
        private readonly IEventAggregator _eventAggregator;

        public HttpMessagingService(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        //TODO Add Dispose - maybe to all IService
        public Task Initialize()
        {
            _eventAggregator.SubscribeForAsyncResult<IHttpMessage>(MessageHandler);
            return Task.CompletedTask;
        }

        public async Task<object> MessageHandler(IMessageEnvelope<IHttpMessage> message)
        {
            var httpMessage = message.Message;

            if (httpMessage.RequestType == "POST")
            {
                return await SendPostRequest(httpMessage).ConfigureAwait(false);
            }
            else
            if (httpMessage.RequestType == "GET")
            {
                return await SendGetRequest(httpMessage).ConfigureAwait(false);
            }

            return null;
        }

        public async Task<object> SendGetRequest(IHttpMessage httpMessage)
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

        public async Task<object> SendPostRequest(IHttpMessage httpMessage)
        {
            var address = httpMessage.MessageAddress();
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
                var response = await httpClient.PostAsync(address, content).ConfigureAwait(false);
                var responseBody = await response.Content.ReadAsStringAsync(Encoding.UTF8).ConfigureAwait(false);

                return httpMessage.ParseResult(responseBody);
            }
        }
    }
}
