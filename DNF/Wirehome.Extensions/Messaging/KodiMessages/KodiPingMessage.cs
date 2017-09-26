using Wirehome.Extensions.Messaging.Services;
using Newtonsoft.Json;
using System;

namespace Wirehome.Extensions.Messaging.KodiMessages
{
    public class KodiPingMessage : HttpMessage
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Method { get; set; }
        public object Parameters { get; set; }

        public override string MessageAddress()
        {
            Creditionals = new System.Net.NetworkCredential(UserName, Password);

            var uri = new Uri($"http://{Address}");
            return $"http://{uri.Host}:{uri.Port}/jsonrpc";
        }

        public override string Serialize()
        {
            var jsonRpcRequest = new JsonRpcRequest
            {
                Method = Method,
                Parameters = new object()
            };

            return jsonRpcRequest.ToString();
        }

        public override object ParseResult(string responseData)
        {
            var result = JsonConvert.DeserializeObject<JsonRpcResponse<JsonPausePlayResult>>(responseData);


            //TODO
            return "";
        }
    }
}
