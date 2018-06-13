using Newtonsoft.Json;
using System.Collections.Generic;

namespace Wirehome.Core.ComponentModel.Configuration
{
    public class ConditionContainerDTO
    {
        [JsonProperty("Expression")]
        public string Expression { get; set; }

        [JsonProperty("Conditions")]
        public IList<ConditionDTO> Conditions { get; set; }
    }
}