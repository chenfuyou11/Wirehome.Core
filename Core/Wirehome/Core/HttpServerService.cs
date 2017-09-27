using System;
using System.IO;
using System.Text;
using Wirehome.Api.Configuration;
using Wirehome.Contracts.Api;
using Wirehome.Contracts.Components;
using Wirehome.Contracts.Configuration;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Logging;
using Wirehome.Contracts.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using Wirehome.Contracts.Network.Http;
using Wirehome.Contracts.Network.Websockets;

namespace Wirehome.Api
{
    public class HttpServerService : ServiceBase, IApiAdapter, IHttpServerService
    {
        private readonly ILogger _log;
        private readonly IHttpServer _httpServer;

        private const string ApiBaseUri = "/api/";
        private const string AppBaseUri = "/app/";
        private const string ManagementAppBaseUri = "/managementApp/";
        

        public HttpServerService(IConfigurationService configurationService, IApiDispatcherService apiDispatcherService, ILogService logService, IHttpServer httpServer)
        {
            if (configurationService == null) throw new ArgumentNullException(nameof(configurationService));
            if (apiDispatcherService == null) throw new ArgumentNullException(nameof(apiDispatcherService));
            _log = logService.CreatePublisher(nameof(HttpServerService)) ?? throw new ArgumentNullException(nameof(logService));
            _httpServer = httpServer ?? throw new ArgumentNullException(nameof(httpServer));

            _httpServer.HttpRequestReceived += OnHttpRequestReceived;
            _httpServer.WebSocketConnected += AttachWebSocket;

            var configuration = configurationService.GetConfiguration<HttpServerServiceConfiguration>("HttpServerService");
            _httpServer.BindAsync(configuration.Port).GetAwaiter().GetResult();

            apiDispatcherService.RegisterAdapter(this);
            
        }

        public event EventHandler<ApiRequestReceivedEventArgs> ApiRequestReceived;
        public event EventHandler<HttpRequestReceivedEventArgs> HTTPRequestReceived;

        public void NotifyStateChanged(IComponent component)
        {
        }

        private void OnHttpRequestReceived(object sender, HttpRequestReceivedEventArgs e)
        {
            if (e.Context.Request.Uri.StartsWith(ApiBaseUri, StringComparison.OrdinalIgnoreCase))
            {
                e.IsHandled = true;
                OnApiRequestReceived(e.Context);
            }
            else if (e.Context.Request.Uri.StartsWith(AppBaseUri, StringComparison.OrdinalIgnoreCase))
            {
                e.IsHandled = true;
                OnAppRequestReceived(e.Context, StoragePath.AppRoot);
            }
            else if (e.Context.Request.Uri.StartsWith(ManagementAppBaseUri, StringComparison.OrdinalIgnoreCase))
            {
                e.IsHandled = true;
                OnAppRequestReceived(e.Context, StoragePath.ManagementAppRoot);
            }

            HTTPRequestReceived?.Invoke(sender, e);
        }

        private void OnApiRequestReceived(HttpContext context)
        {
            IApiCall apiCall = CreateApiContext(context);
            if (apiCall == null)
            {
                context.Response.StatusCode = HttpStatusCode.BadRequest;
                return;
            }

            var eventArgs = new ApiRequestReceivedEventArgs(apiCall);
            ApiRequestReceived?.Invoke(this, eventArgs);

            if (!eventArgs.IsHandled)
            {
                context.Response.StatusCode = HttpStatusCode.BadRequest;
                return;
            }

            context.Response.StatusCode = HttpStatusCode.OK;
            if (eventArgs.ApiContext.Result == null)
            {
                eventArgs.ApiContext.Result = new JObject();
            }

            var apiResponse = new ApiResponse
            {
                ResultCode = apiCall.ResultCode,
                Result = apiCall.Result,
                ResultHash = apiCall.ResultHash
            };

            var json = JsonConvert.SerializeObject(apiResponse);
            context.Response.Body = Encoding.UTF8.GetBytes(json);
            context.Response.MimeType = MimeTypeProvider.Json;
        }

        private static void OnAppRequestReceived(HttpContext context, string rootDirectory)
        {
            string filename;
            if (!TryGetFilename(context, rootDirectory, out filename))
            {
                context.Response.StatusCode = HttpStatusCode.BadRequest;
                return;
            }

            if (File.Exists(filename))
            {
                context.Response.Body = File.ReadAllBytes(filename);
                context.Response.MimeType = MimeTypeProvider.GetMimeTypeFromFilename(filename);
            }
            else
            {
                context.Response.StatusCode = HttpStatusCode.NotFound;
            }
        }

        private static bool TryGetFilename(HttpContext context, string rootDirectory, out string filename)
        {
            var relativeUrl = context.Request.Uri;
            relativeUrl = relativeUrl.TrimStart('/');
            relativeUrl = relativeUrl.Substring(relativeUrl.IndexOf('/') + 1);

            if (relativeUrl.EndsWith("/") || relativeUrl == string.Empty)
            {
                relativeUrl += "Index.html";
            }

            relativeUrl = relativeUrl.Trim('/');
            relativeUrl = relativeUrl.Replace("/", @"\");

            filename = Path.Combine(rootDirectory, relativeUrl);
            return true;
        }

        private void AttachWebSocket(object sender, WebSocketConnectedEventArgs e)
        {
            // Accept each URI at the moment.
            e.IsHandled = true;

            e.WebSocketClientSession.MessageReceived += OnWebSocketMessageReceived;
            e.WebSocketClientSession.Closed += (_, __) => e.WebSocketClientSession.MessageReceived -= OnWebSocketMessageReceived;
        }

        private void OnWebSocketMessageReceived(object sender, WebSocketMessageReceivedEventArgs e)
        {
            var requestMessage = JObject.Parse(((WebSocketTextMessage)e.Message).Text);
            var apiRequest = requestMessage.ToObject<ApiRequest>();

            var context = new ApiCall(apiRequest.Action, apiRequest.Parameter, apiRequest.ResultHash);

            var eventArgs = new ApiRequestReceivedEventArgs(context);
            ApiRequestReceived?.Invoke(this, eventArgs);

            if (!eventArgs.IsHandled)
            {
                context.ResultCode = ApiResultCode.ActionNotSupported;
            }

            var apiResponse = new ApiResponse
            {
                ResultCode = context.ResultCode,
                Result = context.Result,
                ResultHash = context.ResultHash
            };

            var jsonResponse = JObject.FromObject(apiResponse);
            jsonResponse["CorrelationId"] = requestMessage["CorrelationId"];

            e.WebSocketClientSession.SendAsync(jsonResponse.ToString()).Wait();
        }

        private ApiCall CreateApiContext(HttpContext context)
        {
            try
            {
                string bodyText;

                // Parse a special query parameter.
                if (!string.IsNullOrEmpty(context.Request.Query) && context.Request.Query.StartsWith("body=", StringComparison.OrdinalIgnoreCase))
                {
                    bodyText = context.Request.Query.Substring("body=".Length);
                }
                else
                {
                    bodyText = Encoding.UTF8.GetString(context.Request.Body ?? new byte[0]);
                }

                var action = context.Request.Uri.Substring("/api/".Length);
                var parameter = string.IsNullOrEmpty(bodyText) ? new JObject() : JObject.Parse(bodyText);

                return new ApiCall(action, parameter, null);
            }
            catch (Exception)
            {
                _log.Verbose("Received a request with no valid JSON request.");
                return null;
            }
        }
    }
}