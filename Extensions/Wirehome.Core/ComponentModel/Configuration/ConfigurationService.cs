using AutoMapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wirehome.ComponentModel.Adapters;
using Wirehome.ComponentModel.Components;
using Wirehome.Core.ComponentModel.Configuration;
using Wirehome.Core.Utils;

namespace Wirehome.ComponentModel.Configuration
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly IMapper _mapper;
        private readonly IAdapterServiceFactory _adapterServiceFactory;

        public ConfigurationService(IMapper mapper, IAdapterServiceFactory adapterServiceFactory)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _adapterServiceFactory = adapterServiceFactory ?? throw new ArgumentNullException(nameof(adapterServiceFactory));
        }

        public async Task<WirehomeConfiguration> ReadConfiguration(string rawConfig)
        {
            var result = JsonConvert.DeserializeObject<WirehomeConfig>(rawConfig);

            return new WirehomeConfiguration
            {
                Adapters = await MapAdapters(result.Wirehome.Adapters).ConfigureAwait(false),
                Components = await MapComponents(result).ConfigureAwait(false)
            };
        }

        private async Task<IList<Component>> MapComponents(WirehomeConfig result)
        {
            var components = _mapper.Map<IList<ComponentDTO>, IList<Component>>(result.Wirehome.Components);
            foreach (var component in components)
            {
                await component.Initialize().ConfigureAwait(false);
            }
            return components;
        }

        private async Task<IList<Adapter>> MapAdapters(IList<AdapterDTO> adapterConfigs)
        {
            var adapters = new List<Adapter>();
            var types = AssemblyHelper.GetAllInherited<Adapter>();

            foreach (var adapterConfig in adapterConfigs)
            {
                var adapterType = types.FirstOrDefault(t => t.Name == adapterConfig.Type);
                if (adapterType == null) throw new Exception($"Could not find adapter {adapterType}");
                var adapter = (Adapter)_mapper.Map(adapterConfig, typeof(AdapterDTO), adapterType);

                await adapter.Initialize().ConfigureAwait(false);
                adapters.Add(adapter);
            }
            
            return adapters;
        }
    }

    
}
