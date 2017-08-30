using HA4IoT.Extensions.Messaging.Core;
using HA4IoT.Extensions.Messaging.Services;
using System;
using System.Collections.Generic;
using System.Net;

namespace HA4IoT.Extensions.Messaging
{
    public class HttpMessage : IHttpMessage
    {
        public string Address { get; set; }
        public Type DefaultService { get; set; } = typeof(HttpMessagingService);

        public string RequestType { get; set; }
        public string ContentType { get; set; }
        public CookieContainer Cookies { get; set; }
        public Dictionary<string, string> DefaultHeaders { get; set; } = new Dictionary<string, string>();
        public KeyValuePair<string, string> AuthorisationHeader { get; set; } = new KeyValuePair<string, string>("", "");
        public NetworkCredential Creditionals { get; set; }

        public virtual string MessageAddress() { return string.Empty; }
        public virtual string Serialize() { return string.Empty; }
        public virtual object ParseResult(string responseData) { return null; }  
    }
}
