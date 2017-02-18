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
using System.Linq;
using System.Collections.Generic;
using HA4IoT.Extensions.MessagesModel;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Extensions.Exceptions;

namespace HA4IoT.Extensions
{
    public class AlexaDispatcherEndpointService : IAlexaDispatcherEndpointService
    {
        private const string API_VERSION = "HA4IoT 1.0";
        private const string PAYLOAD_VERSION = "2";
        private const string MANUFACTURE = "HA4IoT";
        private const string NAMESPACE = "Alexa.ConnectedHome.Control";

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

        private Dictionary<string, IEnumerable<IComponent>> _connectedDevices = new Dictionary<string, IEnumerable<IComponent>>();
        private Dictionary<string, IEnumerable<string>> _aliases = new Dictionary<string, IEnumerable<string>>();

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

        public void AddConnectedVivices(string friendlyName, IEnumerable<IComponent> devices)
        {
            if (_connectedDevices.ContainsKey(friendlyName))
            {
                throw new Exception($"Friendly name '{friendlyName}' is already in use");
            }

            _connectedDevices.Add(friendlyName, devices);
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
            var response = GenerateDiscoveredHeader();

            var devices = new List<Discoveredappliance>();

            foreach (var area in _areService.GetAreas())
            {
                var areaName = area.Settings?.Caption;
                var areaComponents = area.GetComponents<StateMachine>();

                devices.AddRange(GenerateDiscoveredApplianceFromArea(areaName, areaComponents));
            }

            devices.AddRange(GenerateDiscoveredApplianceFromConnectedDevices());

            if (devices.Count == 0)
            {
                return null;
            }

            response.payload.discoveredAppliances = devices.ToArray();

            return response;
        }

        private static DiscoverAppliancesResponse GenerateDiscoveredHeader()
        {
            return new DiscoverAppliancesResponse
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
        }

        private List<Discoveredappliance> GenerateDiscoveredApplianceFromConnectedDevices()
        {
            var devices = new List<Discoveredappliance>();

            foreach (var friendlyName in _connectedDevices.Keys)
            {
                var connectedDevices = _connectedDevices[friendlyName];

                var actions = GetSupportedStates(connectedDevices.Cast<StateMachine>().FirstOrDefault());
                var componentId = $"Composite_{friendlyName.Replace(" ", "_")}";

                if (actions.Count == 0 || string.IsNullOrWhiteSpace(friendlyName))
                {
                    continue;
                }

                devices.Add(new Discoveredappliance()
                {
                    actions = actions.ToArray(),
                    applianceId = componentId,
                    manufacturerName = MANUFACTURE,
                    version = API_VERSION,
                    modelName = "Composite HA4IoT",
                    isReachable = true,
                    friendlyName = friendlyName,
                    friendlyDescription = friendlyName,
                    additionalApplianceDetails = new Additionalappliancedetails
                    {
                        areaName = "None"
                    }
                });
            }

            return devices;
        }

        private List<Discoveredappliance> GenerateDiscoveredApplianceFromArea(string areaName, IList<StateMachine> areaComponents)
        {
            var devices = new List<Discoveredappliance>();

            foreach (var compoment in areaComponents)
            {
                var actions = GetSupportedStates(compoment);
                var componentId = GetCompatibileComponentID(compoment);
                var friendlyName = GetFriendlyName(areaName, compoment);

                if (actions.Count == 0 || string.IsNullOrWhiteSpace(friendlyName))
                {
                    continue;
                }

                devices.Add(new Discoveredappliance()
                {
                    actions = actions.ToArray(),
                    applianceId = componentId,
                    manufacturerName = MANUFACTURE,
                    version = API_VERSION,
                    modelName = compoment.GetType().ToString(),
                    isReachable = true,
                    friendlyName = friendlyName,
                    friendlyDescription = friendlyName,
                    additionalApplianceDetails = new Additionalappliancedetails
                    {
                        areaName = areaName
                    }
                });
            }

            return devices;
        }



        private string GetFriendlyName(string areaName, IComponent compoment)
        {
            string friendlyName = string.Empty;

            var componentSetting = _settingService.GetSettings<ComponentSettings>(compoment.Id);

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

            return friendlyName;
        }

        private static string GetCompatibileComponentID(StateMachine compoment)
        {
            return compoment.Id.Value.Replace(".", "_");
        }

        private List<string> GetSupportedStates(StateMachine compoment)
        {
            var actions = new List<string>();
            foreach (var supportedState in compoment.GetSupportedStates().Select(x => x.ToString()))
            {
                if (_supportedStatesMap.ContainsKey(supportedState))
                {
                    actions.Add(_supportedStatesMap[supportedState]);
                }
            }
            return actions;
        }

        private object PrepareInvokeMessage(ApiContext context)
        {
            TurnRequest request = null;

            try
            {
                request = context.Parameter.ToObject<TurnRequest>();

                var componentID = request?.ComponentID;

                if (string.IsNullOrWhiteSpace(componentID))
                {
                    throw new NotFoundException();
                }

                if (componentID.IndexOf("Composite") > -1)
                {
                    // Cut component prefix
                    componentID = componentID.Substring(10, componentID.Length - 10);
                    componentID = componentID.Replace("_", " ");

                    if (!_connectedDevices.ContainsKey(componentID))
                    {
                        throw new NotFoundException();
                    }

                    foreach (var device in _connectedDevices[componentID])
                    {
                        RunComponentCommand(request.Command, device.Id.Value, true);
                    }
                }
                else
                {
                    componentID = request?.ComponentID?.Replace("_", ".");

                    RunComponentCommand(request.Command, componentID);
                }

                var confirmation_name = _invokeConfirmationMap[request.Command];

                return new TurnConfirmation
                {
                    header = new Header
                    {
                        messageId = request.MessageID,
                        name = confirmation_name,
                        payloadVersion = PAYLOAD_VERSION,
                        _namespace = NAMESPACE
                    },
                    payload = new Payload()
                };
            }
            catch (NotFoundException)
            {
                return new TurnConfirmation
                {
                    header = new Header
                    {
                        messageId = request.MessageID,
                        name = "NoSuchTargetError",
                        payloadVersion = PAYLOAD_VERSION,
                        _namespace = NAMESPACE
                    },
                    payload = new Payload()
                };
            }
            catch (StateAlreadySetException)
            {
                return new NotSupportedInCurrentModeError 
                {
                    header = new Header
                    {
                        messageId = request.MessageID,
                        name = "NotSupportedInCurrentModeError",
                        payloadVersion = PAYLOAD_VERSION,
                        _namespace = NAMESPACE
                    },
                    payload = new ErrorPayload
                    {
                        currentDeviceMode = "OTHER"
                    }
                };
            }
        }

        private void RunComponentCommand(string command, string componentID, bool ignoreCurrentStateCheck = false)
        {
            var component = _componentService.GetComponent(new ComponentId(componentID)) as IActuator;

            if (component != null && _invokeCommandMap.ContainsKey(command))
            {
                var requested_state = new ComponentState(_invokeCommandMap[command]);

                if (!ignoreCurrentStateCheck)
                {
                    var currentState = component.GetState();

                    if (currentState.Equals(requested_state))
                    {
                        throw new StateAlreadySetException();
                    }
                }

                component.SetState(requested_state);
            }
        }
    }






   
   





}
