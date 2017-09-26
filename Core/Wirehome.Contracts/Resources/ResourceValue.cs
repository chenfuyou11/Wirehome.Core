using Newtonsoft.Json;

namespace Wirehome.Contracts.Resources
{
    public class ResourceValue
    {
        [JsonRequired]
        public string LanguageCode { get; set; }

        [JsonRequired]
        public string Value { get; set; }
    }
}
