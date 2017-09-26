using Newtonsoft.Json.Linq;

namespace Wirehome.Contracts.Api
{
    public class ApiRequest
    {
        public string Action { get; set; }
        public JObject Parameter { get; set; }
        public string ResultHash { get; set; }
    }
}
