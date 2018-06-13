﻿using Newtonsoft.Json;
using System.Collections.Generic;
using Wirehome.ComponentModel;

namespace Wirehome.Core.ComponentModel.Configuration
{
    public class ScheduleDTO
    {
        [JsonProperty("Uid")]
        public string Uid { get; set; }

        [JsonProperty("Type")]
        public string Type { get; set; }

        [JsonProperty("Properties")]
        [JsonConverter(typeof(PropertyDictionaryConverter))]
        public Dictionary<string, Property> Properties { get; set; }

        [JsonProperty("Tags")]
        public IDictionary<string, string> Tags { get; set; }

        [JsonProperty("CronExpression")]
        public string CronExpression { get; set; }

        [JsonProperty("Calendar")]
        public string Calendar { get; set; }

        [JsonProperty("ManualSchedules")]
        public IList<ManualScheduleDTO> ManualSchedules { get; set; }
    }
}