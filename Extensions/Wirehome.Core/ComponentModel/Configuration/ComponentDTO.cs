using System.Collections.Generic;
using Newtonsoft.Json;
using Wirehome.ComponentModel.Components;

namespace Wirehome.Core.ComponentModel.Configuration
{
    public class ComponentDTO
    {

        [JsonProperty("Uid")]
        public string Uid { get; set; }

        [JsonProperty("IsEnabled")]
        public bool IsEnabled { get; set; }

        [JsonProperty("AdapterRefs")]
        public IList<AdapterReferenceDTO> Adapters { get; set; }

        [JsonProperty("Converters")]
        [JsonConverter(typeof(ValueConverter))]
        public IDictionary<string, IValueConverter> Converters { get; set; }

        [JsonProperty("Tags")]
        public IDictionary<string, string> Tags { get; set; }

        [JsonProperty("Classes")]
        public IList<string> Classes { get; set; }

        public string Type { get; set; } = "Test";
    }


}
