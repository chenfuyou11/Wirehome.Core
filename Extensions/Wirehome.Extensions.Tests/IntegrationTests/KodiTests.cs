using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Quartz;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Wirehome.Contracts.Components.Commands;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Logging;
using Wirehome.Core;
using Wirehome.Extensions.Devices;
using Wirehome.Extensions.Devices.Kodi;
using Wirehome.Extensions.Messaging.Core;
using Wirehome.Extensions.Messaging.KodiMessages;
using Wirehome.Extensions.Messaging.Services;
using Wirehome.Extensions.Quartz;

namespace Wirehome.Extensions.Tests
{
    [TestClass]
    [TestCategory("Integration")]
    public class KodiTests
    {
        public const string KODI_HOST = "192.168.0.159";
        public const string KODI_USER = "kodi";
        public const string KODI_PASS = "9dominik";
        public const int KODI_PORT = 8080;

        private (IEventAggregator ev, IScheduler ch) PrepareMocks()
        {
            var log = Mock.Of<ILogService>();

            var container = new Container(new ControllerOptions());
            container.RegisterSingleton<IContainer>(() => container);
            container.RegisterSingleton<IEventAggregator, EventAggregator>();
            container.RegisterSingleton(log);
            container.RegisterSingleton<DenonStateJob>();
            container.RegisterQuartz();

            var scheduler = container.GetInstance<IScheduler>();
            var eventAggregator = container.GetInstance<IEventAggregator>();

            var http = new HttpMessagingService(eventAggregator);
            http.Initialize();

            return (eventAggregator, scheduler);
        }


        [TestMethod]
        public async Task KodiPowerTest()
        {
            var mocks = PrepareMocks();

            var denon = new KodiDevice(Guid.NewGuid().ToString(), mocks.ev, mocks.ch)
            {
                StatusInterval = TimeSpan.FromMilliseconds(500),
                Hostname = KODI_HOST,
                UserName = KODI_USER,
                Password = KODI_PASS,
                Port= KODI_PORT
            };

            await denon.Initialize();

            await denon.ExecuteAsyncCommand<TurnOnCommand>();

        }

        [TestMethod]
        public async Task TestKod()
        {
            //http://kodi.wiki/view/JSON-RPC_API/Examples#Introspect
            //http://kodi.wiki/view/JSON-RPC_API

            var ip = "192.168.0.159";
            var port = 8080;
            var uri = $"http://{ip}:{port}/jsonrpc";
           // Dictionary<string, string> DefaultHeaders = new Dictionary<string, string>();
           // KeyValuePair<string, string> AuthorisationHeader = new KeyValuePair<string, string>("", "");

            // DefaultHeaders.Add("Content-Type", "application/json-rpc");
           // AuthorisationHeader = new KeyValuePair<string, string>("", "");

            var jsonRpcRequest = new JsonRpcRequest
            {
                Method = "JSONRPC.Ping",
                Parameters = new object()
            };

            //var jsonRpcRequest = new JsonRpcRequest
            //{
            //    Method = "Player.PlayPause",
            //    Parameters = new { playerid = 1 }
            //};

            //var jsonRpcRequest = new JsonRpcRequest
            //{
            //    Id = "1",
            //    Method = "Player.GetActivePlayers",
            //    Parameters = new object()
            //};


            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.Credentials = new NetworkCredential("kodi", "9dominik");


            using (var httpClient = new HttpClient(httpClientHandler))
            {
                //foreach (var header in DefaultHeaders)
                //{
                //    httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                //}

                //if (!string.IsNullOrWhiteSpace(AuthorisationHeader.Key))
                //{
                //    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AuthorisationHeader.Key, AuthorisationHeader.Value);
                //}

                var content = new StringContent(jsonRpcRequest.ToString());
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json-rpc");

                var response = await httpClient.PostAsync(uri, content).ConfigureAwait(false);
                var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                //var result = JsonConvert.DeserializeObject<JsonRpcResponse<JsonPausePlayResult>>(responseBody);

            }
        }

    }
}

