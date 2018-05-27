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

        [JsonProperty("Schedule")]
        public ScheduleDTO Schedule { get; set; }

        [JsonProperty("Condition")]
        public ConditionDTO Condition { get; set; }
    }
}
