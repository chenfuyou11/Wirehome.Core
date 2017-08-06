using System.Collections.Generic;
using System.Net;

namespace HA4IoT.Extensions.Messaging
{
    public class HttpMessage : BaseMessage
    {
        public CookieContainer Cookies { get; set; }
        public string RequestType {get; set;}
        public Dictionary<string, string> DefaultHeaders { get; set; } = new Dictionary<string, string>();
        public KeyValuePair<string, string> AuthorisationHeader { get; set; } = new KeyValuePair<string, string>("", "");

        public virtual string GetAddress() { return string.Empty; }
        public virtual string Serialize() { return string.Empty; }
        public virtual void ValidateResponse(string responseData) { }
    }
}
