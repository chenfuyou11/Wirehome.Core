using Newtonsoft.Json;

namespace HA4IoT.Extensions
{

    public class Header
    {
        public string messageId { get; set; }
        public string name { get; set; }
        [JsonProperty(PropertyName = "namespace")]
        public string _namespace { get; set; }
        public string payloadVersion { get; set; }
    }

}
