using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Wirehome.Alexa.Service
{
    public class Function
    {
        
        public object FunctionHandler(DiscoverRequest request, ILambdaContext context)
        {
            try
            {
                LambdaLogger.Log(request.Directive.Header.Namespace + Environment.NewLine);
                if(request.Directive.Header.Name == "Discover")
                {
                    var device = new Endpoint
                    {
                        EndpointId = "device-1",
                        FriendlyName = "Dominik",
                        Description = "Use it in home",
                        ManufacturerName = "DNF",
                        DisplayCategories = new[] { "LIGHT" },
                        Cookie = new Cookie { ExtraDetail1 = "Extra" },
                        Capabilities = new[]
                        {
                            new Capability
                            {
                                Type = "AlexaInterface",
                                Interface = "Alexa.PowerController",
                                Version = "3",
                                ProactivelyReported = true,
                                SupportsDeactivation = true,
                                Retrievable = true,
                                Properties = new Properties
                                {
                                    Supported = new[]
                                    {
                                        new Supported
                                        {
                                            Name = "powerState"
                                        }
                                    }
                                }
                            }
                        }
                        
                    };

                    var response = new DiscoverResponse
                    {
                        Event = new Event
                        {
                            Header = request.Directive.Header,
                            Payload = new DiscoveryResponsePayload
                            {
                                Endpoints = new List<Endpoint> { device }
                            }
                        }
                    };
                    response.Event.Header.Name = "Discover.Response";

                    return response;

                }
            }
            catch (Exception ex)
            {
                LambdaLogger.Log(ex.ToString());
            }
            return request;

        }
    }


    public class Header
    {

        [JsonProperty("namespace")]
        public string Namespace { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("payloadVersion")]
        public string PayloadVersion { get; set; }

        [JsonProperty("messageId")]
        public string MessageId { get; set; }
    }

    public class Scope
    {

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }
    }

    public class Payload
    {

        [JsonProperty("scope")]
        public Scope Scope { get; set; }
    }

    public class Directive
    {

        [JsonProperty("header")]
        public Header Header { get; set; }

        [JsonProperty("payload")]
        public Payload Payload { get; set; }
    }

    public class DiscoverRequest
    {

        [JsonProperty("directive")]
        public Directive Directive { get; set; }
    }

    

    public class Cookie
    {

        [JsonProperty("extraDetail1")]
        public string ExtraDetail1 { get; set; }

        [JsonProperty("extraDetail2")]
        public string ExtraDetail2 { get; set; }

        [JsonProperty("extraDetail3")]
        public string ExtraDetail3 { get; set; }

        [JsonProperty("extraDetail4")]
        public string ExtraDetail4 { get; set; }
    }

    public class Supported
    {

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Properties
    {

        [JsonProperty("supported")]
        public IList<Supported> Supported { get; set; }

        [JsonProperty("proactivelyReported")]
        public bool ProactivelyReported { get; set; }

        [JsonProperty("retrievable")]
        public bool Retrievable { get; set; }
    }

    public class Resolution
    {

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }
    }

    public class CameraStreamConfiguration
    {

        [JsonProperty("protocols")]
        public IList<string> Protocols { get; set; }

        [JsonProperty("resolutions")]
        public IList<Resolution> Resolutions { get; set; }

        [JsonProperty("authorizationTypes")]
        public IList<string> AuthorizationTypes { get; set; }

        [JsonProperty("videoCodecs")]
        public IList<string> VideoCodecs { get; set; }

        [JsonProperty("audioCodecs")]
        public IList<string> AudioCodecs { get; set; }
    }

    public class Capability
    {

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("interface")]
        public string Interface { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("properties")]
        public Properties Properties { get; set; }

        [JsonProperty("supportsDeactivation")]
        public bool? SupportsDeactivation { get; set; }

        [JsonProperty("proactivelyReported")]
        public bool? ProactivelyReported { get; set; }

        [JsonProperty("retrievable")]
        public bool? Retrievable { get; set; }

        [JsonProperty("cameraStreamConfigurations")]
        public IList<CameraStreamConfiguration> CameraStreamConfigurations { get; set; }
    }

    public class Endpoint
    {

        [JsonProperty("endpointId")]
        public string EndpointId { get; set; }

        [JsonProperty("friendlyName")]
        public string FriendlyName { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("manufacturerName")]
        public string ManufacturerName { get; set; }

        [JsonProperty("displayCategories")]
        public IList<string> DisplayCategories { get; set; }

        [JsonProperty("cookie")]
        public Cookie Cookie { get; set; }

        [JsonProperty("capabilities")]
        public IList<Capability> Capabilities { get; set; }
    }

    public class DiscoveryResponsePayload
    {

        [JsonProperty("endpoints")]
        public IList<Endpoint> Endpoints { get; set; }
    }

    public class Event
    {

        [JsonProperty("header")]
        public Header Header { get; set; }

        [JsonProperty("payload")]
        public DiscoveryResponsePayload Payload { get; set; }
    }

    public class DiscoverResponse
    {

        [JsonProperty("event")]
        public Event Event { get; set; }
    }

}
