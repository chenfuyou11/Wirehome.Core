using System;
using System.Collections.Generic;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Wirehome.Alexa.Model;
using Wirehome.Alexa.Model.Common;
using Wirehome.Alexa.Model.Discovery;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Wirehome.Alexa.Service
{
    public class Function
    {
        public object FunctionHandler(object request, ILambdaContext context)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<SmartHomeRequest>(request.ToString());

                //LambdaLogger.Log(request.Directive.Header.Namespace + Environment.NewLine);
                //if(request.Directive.Header.Name == "Discover")
                //{
                //    var device = new Endpoint
                //    {
                //        EndpointId = "device-1",
                //        FriendlyName = "Dominik3",
                //        Description = "Use it in home",
                //        ManufacturerName = "DNF",
                //        DisplayCategories = new[] { nameof(DisplayCategory.LIGHT) },
                //        Cookie = new Cookie { ExtraDetail1 = "Extra" },
                //        Capabilities = new[]
                //        {
                //            new Capability
                //            {
                //                Interface = "Alexa.PowerController",
                //                ProactivelyReported = true,
                //                SupportsDeactivation = true,
                //                Retrievable = true,
                //                Properties = new Properties
                //                {
                //                    Supported = new[]
                //                    {
                //                        new Supported
                //                        {
                //                            Name = "powerState"
                //                        }
                //                    }
                //                }
                //            }
                //        }

                //    };

                //    var response = new DiscoverResponse
                //    {
                //        Event = new Event
                //        {
                //            Header = request.Directive.Header,
                //            Payload = new DiscoveryResponsePayload
                //            {
                //                Endpoints = new List<Endpoint> { device }
                //            }
                //        }
                //    };
                //    response.Event.Header.Name = "Discover.Response";

                //    LambdaLogger.Log(JsonConvert.SerializeObject(response) + Environment.NewLine);

                //    return response;

                //}
            }
            catch (Exception ex)
            {
                LambdaLogger.Log(ex.ToString());
            }
            return request;
        }
    }
}
