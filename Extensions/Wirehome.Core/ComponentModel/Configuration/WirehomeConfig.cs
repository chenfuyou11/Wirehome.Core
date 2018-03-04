using Newtonsoft.Json;

namespace Wirehome.Core.ComponentModel.Configuration
{
    public class WirehomeConfig
    {
        [JsonProperty("Wirehome")]
        public WirehomeRoot Wirehome { get; set; }
    }


}
