using Wirehome.Contracts.Hardware.RemoteSockets.Protocols;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Wirehome.Contracts.Hardware.RemoteSockets.Configuration
{
    public sealed class IntertechnoConfiguration
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public IntertechnoSystemCode SystemCode { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public IntertechnoUnitCode UnitCode { get; set; }
    }
}
