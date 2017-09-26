using Newtonsoft.Json;

namespace Wirehome.Extensions.Messaging.KodiMessages
{
    public class Data
    {
        [JsonProperty(PropertyName = "method")]
        public string Method { get; set; }

        [JsonProperty(PropertyName = "stack")]
        public Stack Stack { get; set; }
    }
    
}
