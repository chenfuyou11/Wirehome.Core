using Newtonsoft.Json.Linq;

namespace Wirehome.Contracts.Hardware.RemoteSockets.Configuration
{
    public sealed class RemoteSocketCodeGeneratorConfiguration
    {
        public string Type { get; set; }

        public JObject Parameters { get; set; } = new JObject();
    }
}
