using System;
using System.Collections.Generic;
using System.Linq;
using Wirehome.Contracts.Api;
using Wirehome.Contracts.Areas;
using Wirehome.Contracts.Components;
using Wirehome.Contracts.Logging;
using Wirehome.Extensions.Exceptions;
using Wirehome.Extensions.MessagesModel;
using Wirehome.Contracts.Components.Features;
using Wirehome.Contracts.Settings;
using Wirehome.Contracts.Components.Commands;
using Wirehome.Extensions.Contracts;
using Wirehome.Contracts.Core;
using System.Threading.Tasks;
using HTTPnet.Core.Http;
using HTTPnet.Core.Pipeline;

namespace Wirehome.Extensions
{
    public class AlexaDispatcherEndpointService : IAlexaDispatcherEndpointService, IHttpContextPipelineHandler
    {
        private const string API_VERSION = "Wirehome 1.0";
        private const string PAYLOAD_VERSION = "2";
        private const string MANUFACTURE = "Wirehome";
        private const string NAMESPACE = "Alexa.ConnectedHome.Control";

        private readonly IHttpServerService _httpServer;
        private readonly IAreaRegistryService _areService;
        private readonly ISettingsService _settingService;
        private readonly IComponentRegistryService _componentService;
        private readonly ILogger _log;

        private readonly Dictionary<string, ICommand> _invokeCommandMap = new Dictionary<string, ICommand>
        {
            { "TurnOnRequest", new TurnOnCommand() },
            {"TurnOffRequest", new TurnOffCommand() }
        };

        private Dictionary<string, string> _invokeConfirmationMap = new Dictionary<string, string>
        {
            { "TurnOnRequest", "TurnOnConfirmation" },
            {"TurnOffRequest", "TurnOffConfirmation" }
        };

        private Dictionary<string, IEnumerable<IComponent>> _connectedDevices = new Dictionary<string, IEnumerable<IComponent>>();
        private Dictionary<string, IEnumerable<string>> _aliases = new Dictionary<string, IEnumerable<string>>();

        public AlexaDispatcherEndpointService(IHttpServerService httpServer, IAreaRegistryService areService, ISettingsService settingService, 
            IComponentRegistryService componentService, ILogService logService)
        {
            _httpServer = httpServer ?? throw new ArgumentNullException(nameof(httpServer));
            _areService = areService ?? throw new ArgumentNullException(nameof(areService));
            _settingService = settingService ?? throw new ArgumentNullException(nameof(settingService));
            _componentService = componentService ?? throw new ArgumentNullException(nameof(componentService));

            _log = logService.CreatePublisher(nameof(AlexaDispatcherEndpointService));
        }

        public Task Initialize()
        {
            _httpServer.AddRequestHandler(this);

            return Task.CompletedTask;
        }

        //TODO Fix
        public Task ProcessRequestAsync(HttpContextPipelineHandlerContext context)
        {
            return Task.CompletedTask;
        }

        //TODO Fix
        public Task ProcessResponseAsync(HttpContextPipelineHandlerContext context)
        {
            return Task.CompletedTask;
        }

        public void AddConnectedVivices(string friendlyName, IEnumerable<IComponent> devices)
        {
            if (_connectedDevices.ContainsKey(friendlyName))
            {
                throw new Exception($"Friendly name '{friendlyName}' is already in use");
            }

            _connectedDevices.Add(friendlyName, devices);
        }

        private void DispatchHttpRequest(object sender, HttpContextPipelineHandlerContext eventArgs)
        {
            if (!eventArgs.HttpContext.Request.Uri.StartsWith("/alexa/"))
            {
                return;
            }

            HandleHttpRequest(eventArgs.HttpContext);
            eventArgs.BreakPipeline = true;
        }

        private void HandleHttpRequest(HttpContext httpContext)
        {
            //TODO FIX
            //var apiContext = CreateApiContext(httpContext);
            //if (apiContext == null)
            //{
            //    httpContext.Response.StatusCode = HttpStatusCode.BadRequest;
            //    return;
            //}

            //var response = DispatchHttpRequest(apiContext);
            //apiContext.Result = response != null ? JObject.FromObject(response) : new JObject();

            //var json = JsonConvert.SerializeObject(apiContext.Result);
            //httpContext.Response.Body = Encoding.UTF8.GetBytes(json);
            //httpContext.Response.MimeType = MimeTypeProvider.Json;
        }

        private ApiCall CreateApiContext(HttpContext httpContext)
        {
            try
            {
                //string bodyText = Encoding.UTF8.GetString(httpContext.Request.Body);

                //int bodyStart = bodyText.IndexOf("{");
                //int bodyEnd = bodyText.LastIndexOf("}");

                //if(bodyStart == -1 || bodyEnd == -1)
                //{
                //    throw new Exception("JSON body is not correctly formmated");
                //}

                //bodyText = bodyText.Substring(bodyStart, bodyEnd - bodyStart + 1)?.Trim();


                //var action = httpContext.Request.Uri.Substring("/alexa/".Length);
                //var parameter = string.IsNullOrEmpty(bodyText) ? new JObject() : JObject.Parse(bodyText);

                //return new ApiCall(action, parameter, null);
                return null;
            }
            catch (Exception)
            {
                _log.Verbose("Received a request with no valid JSON request.");

                return null;
            }
        }

        private object DispatchHttpRequest(ApiCall context)
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

        private object PrepareDicsoverMessage(ApiCall context)
        {
            var response = GenerateDiscoveredHeader();

            var devices = new List<Discoveredappliance>();

            foreach (var area in _areService.GetAreas())
            {
                var areaName = area.Settings?.Caption;
                var areaComponents = area.GetComponents();

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

                var actions = GetSupportedStates(connectedDevices.Cast<IComponent>().FirstOrDefault());
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
                    modelName = "Composite Wirehome",
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

        private List<Discoveredappliance> GenerateDiscoveredApplianceFromArea(string areaName, IList<IComponent> areaComponents)
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
                    friendlyName = compoment.Id.Replace(".", " ");
                }
                else
                {
                    friendlyName = $"{areaName} {componentName}";
                }
            }

            return friendlyName;
        }

        private static string GetCompatibileComponentID(IComponent compoment)
        {
            return compoment.Id.Replace(".", "_");
        }

        private List<string> GetSupportedStates(IComponent component)
        {
            var actions = new List<string>();

            if(component.GetFeatures().Supports<PowerStateFeature>())
            {
                actions.Add("turnOn");
                actions.Add("turnOff");
            }

            return actions;
        }

        private object PrepareInvokeMessage(ApiCall context)
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
                        RunComponentCommand(request.Command, device.Id, true);
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
            var component = _componentService.GetComponent(componentID);

            if (component != null && _invokeCommandMap.ContainsKey(command))
            {
                var requested_state = _invokeCommandMap[command];
                
                component.ExecuteCommand(requested_state);
            }
        }

       
    }

}
