using Newtonsoft.Json.Linq;

namespace Wirehome.Contracts.Api
{
    public interface IApiCall
    {
        string Action { get; }
        JObject Parameter { get; }
        
        ApiResultCode ResultCode { get; set; }
        JObject Result { get; set; }

        string ResultHash { get; set; }
    }
}
