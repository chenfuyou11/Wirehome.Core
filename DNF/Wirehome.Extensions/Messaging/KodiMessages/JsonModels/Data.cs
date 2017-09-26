using Newtonsoft.Json;

namespace HA4IoT.Extensions.Messaging.KodiMessages
{
    public class Data
    {
        [JsonProperty(PropertyName = "method")]
        public string Method { get; set; }

        [JsonProperty(PropertyName = "stack")]
        public Stack Stack { get; set; }
    }
    
}
