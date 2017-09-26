using Newtonsoft.Json.Linq;

namespace Wirehome.Settings
{
    public class SettingsServiceApiRequest
    {
        public string Uri { get; set; }

        public JObject Settings { get; set; }
    }
}
