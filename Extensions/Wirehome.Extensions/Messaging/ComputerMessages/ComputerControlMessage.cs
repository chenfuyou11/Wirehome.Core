using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;

namespace Wirehome.Extensions.Messaging.ComputerMessages
{
    public class ComputerControlMessage : HttpMessage
    {
        public int Port { get; set; } = 5000;
        public string Service { get; set; }
        public object Message { get; set; }

        public ComputerControlMessage()
        {
            RequestType = "POST";
            ContentType = "application/json";
        }

        public override string MessageAddress()
        {
            return $"http://{Address}:{Port}/api/{Service}";
        }

        public override string Serialize()
        {
            if (Message == null) return "";
            return JsonConvert.SerializeObject(Message);
        }

        public override object ParseResult(string responseData, Type responseType = null)
        {
            if(string.IsNullOrWhiteSpace(responseData)) return string.Empty;

            return JsonConvert.DeserializeObject(responseData, responseType);
        }
    }
}
