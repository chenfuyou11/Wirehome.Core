using System.Collections.Generic;
using Newtonsoft.Json;
using Wirehome.ComponentModel.Components;

namespace Wirehome.Core.ComponentModel.Configuration
{

    public class TriggerDTO
    {

        [JsonProperty("Event")]
        public EventDTO Event { get; set; }

        [JsonProperty("Command")]
        public CommandDTO Command { get; set; }
    }


}
