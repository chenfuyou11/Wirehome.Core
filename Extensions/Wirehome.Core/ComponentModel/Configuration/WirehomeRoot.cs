using System.Collections.Generic;
using Newtonsoft.Json;

namespace Wirehome.Core.ComponentModel.Configuration
{
    public class WirehomeRoot
    {

        [JsonProperty("Components")]
        public IList<ComponentDTO> Components { get; set; }

        [JsonProperty("Adapters")]
        public IList<AdapterDTO> Adapters { get; set; }
    }


}
