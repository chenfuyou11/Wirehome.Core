using Wirehome.Contracts.Services;
using Newtonsoft.Json.Linq;

namespace Wirehome.Contracts.Configuration
{
    public interface IConfigurationService : IService
    {
        JObject GetSection(string name);

        TSection GetConfiguration<TSection>(string name) where TSection : class;
    }
}
