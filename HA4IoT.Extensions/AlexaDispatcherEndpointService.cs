using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Networking.Http;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.Settings;
using HA4IoT.Networking.Http;
using HA4IoT.Networking.Json;
using Newtonsoft.Json.Linq;
using HA4IoT.Actuators.StateMachines;
using System;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.Generic;
using HA4IoT.Extensions.MessagesModel;
using HA4IoT.Contracts.Actuators;

namespace HA4IoT.Extensions
{
    public class AlexaDispatcherEndpointService : IService
    {
        private const string API_VERSION = "HA4IoT 1.0";
        private const string PAYLOAD_VERSION = "2";
        private readonly HttpServer _httpServer;
        private readonly IAreaService _areService;
        private readonly ISettingsService _settingService;
        private readonly IComponentService _componentService;

        private Dictionary<string, string> _supportedStatesMap = new Dictionary<string, string>
        {
            { "On", "turnOn" },
            {"Off", "turnOff" }
        };

        private Dictionary<string, string> _invokeCommandMap = new Dictionary<string, string>
        {
            { "TurnOnRequest", "On" },
            {"TurnOffRequest", "Off" }
        };

        private Dictionary<string, string> _invokeConfirmationMap = new Dictionary<string, string>
        {
            { "TurnOnRequest", "TurnOnConfirmation" },
            {"TurnOffRequest", "TurnOffConfirmation" }
        };

        public AlexaDispatcherEndpointService(HttpServer httpServer, IAreaService areService, ISettingsService settingService, IComponentService componentService)
        {
            if (httpServer == null) throw new ArgumentNullException(nameof(httpServer));
            if (areService == null) throw new ArgumentNullException(nameof(areService));
            if (settingService == null) throw new ArgumentNullException(nameof(settingService));
            if (componentService == null) throw new ArgumentNullException(nameof(componentService));

            _httpServer = httpServer;
            _areService = areService;
            _settingService = settingService;
            _componentService = componentService;
        }

        public void Startup()
        {
            _httpServer.RequestReceived += DispatchHttpRequest;
        }

        private void DispatchHttpRequest(object sender, HttpRequestReceivedEventArgs eventArgs)
        {
            if (!eventArgs.Context.Request.Uri.StartsWith("/alexa/"))
            {
                return;
            }

            HandleHttpRequest(eventArgs.Context);
            eventArgs.IsHandled = true;
        }

        private void HandleHttpRequest(HttpContext httpContext)
        {
            var apiContext = CreateApiContext(httpContext);
            if (apiContext == null)
            {
                httpContext.Response.StatusCode = HttpStatusCode.BadRequest;
                return;
            }

            var response = DispatchHttpRequest(apiContext);
            apiContext.Response = response != null ? JObject.FromObject(response) : new JObject();

            httpContext.Response.StatusCode = response != null ? HttpStatusCode.OK : HttpStatusCode.NotFound;
            httpContext.Response.Body = new JsonBody(apiContext.Response);
        }

        private ApiContext CreateApiContext(HttpContext httpContext)
        {
            try
            {
                var request = string.IsNullOrEmpty(httpContext.Request.Body) ? new JObject() : JObject.Parse(httpContext.Request.Body);
                return new ApiContext(httpContext.Request.Uri, request, new JObject());
            }
            catch (Exception)
            {
                Log.Verbose("Received a request with no valid JSON request.");

                return null;
            }
        }

        private object DispatchHttpRequest(ApiContext context)
        {
            if (context.Action.IndexOf("discover") > -1)
            {
                return PrepareDicsoverMessage(context);
            }
            else if (context.Action.IndexOf("invoke") > -1)
            {
                return PrepareInvokeMessage(context);
            }

            return null;
        }

        private object PrepareDicsoverMessage(ApiContext context)
        {
            var response = new DiscoverAppliancesResponse
            {
                header = new Header
                {
                    messageId = Guid.NewGuid().ToString(),
                    name = "DiscoverAppliancesResponse",
                    payloadVersion = PAYLOAD_VERSION,
                    _namespace = "Alexa.ConnectedHome.Discovery"
                },
                payload = new Payload()
            };

            var devices = new List<Discoveredappliance>();

            foreach (var area in _areService.GetAreas())
            {
                var areaName = area.Settings?.Caption;
                var friendlyName = string.Empty;

                foreach (var compoment in area.GetComponents<StateMachine>())
                {
                    var actions = new List<string>();
                    foreach (var supportedState in compoment.GetSupportedStates().Select(x => x.ToString()))
                    {
                        if (_supportedStatesMap.ContainsKey(supportedState))
                        {
                            actions.Add(_supportedStatesMap[supportedState]);
                        }
                    }

                    var componentSetting = _settingService.GetSettings<ComponentSettings>(compoment.Id);
                    var componentId = compoment.Id.Value.Replace(".", "_");

                    if (componentSetting != null)
                    {
                        var componentName = componentSetting.Caption;
                        if (string.IsNullOrWhiteSpace(componentName) || string.IsNullOrWhiteSpace(areaName))
                        {
                            friendlyName = compoment.Id.Value.Replace(".", " ");
                        }
                        else
                        {
                            friendlyName = $"{areaName} {componentName}";
                        }
                    }

                    if (actions.Count == 0 || string.IsNullOrWhiteSpace(friendlyName))
                    {
                        continue;
                    }

                    devices.Add(new Discoveredappliance()
                    {
                        actions = actions.ToArray(),
                        applianceId = componentId,
                        manufacturerName = "HA4IoT",
                        version = API_VERSION,
                        modelName = compoment.GetType().ToString(),
                        isReachable = true,
                        friendlyName = friendlyName,
                        friendlyDescription = friendlyName,
                        additionalApplianceDetails = new Additionalappliancedetails
                        {
                            extendedInfo = "Test"
                        }
                    });
                }
            }

            if (devices.Count == 0)
            {
                return null;
            }

            response.payload.discoveredAppliances = devices.ToArray();

            return response;
        }

        private object PrepareInvokeMessage(ApiContext context)
        {
            var request = context.Parameter.ToObject<TurnRequest>();

            var componentID = request?.ComponentID?.Replace("_", ".");

            var component =_componentService.GetComponent(new ComponentId(componentID)) as IActuator; 

            if (component != null && _invokeCommandMap.ContainsKey(request.Command))
            {
                var requested_state = _invokeCommandMap[request.Command];
                component.SetState(new ComponentState(requested_state));
            }

            var confirmation_name = _invokeConfirmationMap[request.Command];

            var result = new TurnConfirmation
            {
                header = new Header
                {
                    messageId = request.MessageID,
                    name = confirmation_name,
                    payloadVersion = PAYLOAD_VERSION,
                    _namespace = "Alexa.ConnectedHome.Control"
                },
                payload = new Payload()
            };

            return result;
        }
    }




   




}
