using System.Collections.Generic;
using Newtonsoft.Json;
using Wirehome.ComponentModel;

namespace Wirehome.Core.ComponentModel.Configuration
{

    public class AdapterReferenceDTO
    {

        [JsonProperty("Uid")]
        public string Uid { get; set; }

        [JsonProperty("Properties")]
        [JsonConverter(typeof(PropertyDictionaryConverter))]
        public Dictionary<string, Property> Properties { get; set; }
    }


}
