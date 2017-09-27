using Newtonsoft.Json;

namespace Wirehome.Extensions.MessagesModel
{

    public class Header
    {
        public string messageId { get; set; }
        [JsonProperty(PropertyName = "namespace")]
        public string _namespace { get; set; }
        public string name { get; set; }
        public string payloadVersion { get; set; }
    }

}
