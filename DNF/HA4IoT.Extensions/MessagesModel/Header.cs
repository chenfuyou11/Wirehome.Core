using Newtonsoft.Json;

namespace HA4IoT.Extensions.MessagesModel
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
