using Newtonsoft.Json;
using System;

namespace Wirehome.Core.ComponentModel.Configuration
{
    public class ManualScheduleDTO
    {
        [JsonProperty("Start")]
        public DateTime Start { get; set; }

        [JsonProperty("Finish")]
        public DateTime Finish { get; set; }

        [JsonProperty("WorkingTime")]
        public TimeSpan WorkingTime { get; set; }
    }
}