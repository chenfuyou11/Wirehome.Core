using AutoMapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public IList<Component> ReadConfiguration(string rawConfig)
        {
            var result = JsonConvert.DeserializeObject<WirehomeConfig>(rawConfig);
            var components = _mapper.Map<IList<ComponentDTO>, IList<Component>>(result.Wirehome.Components);


            var adapters = new List<Adapter>();
            var types = AssemblyHelper.GetProjectAssemblies()
                                      .SelectMany(s => s.GetTypes())
                                      .Where(p => typeof(Adapter).IsAssignableFrom(p));

            foreach (var adapter in result.Wirehome.Adapters)
            {
                var converterType = types.FirstOrDefault(t => t.Name == adapter.Type);
                if (converterType == null) throw new Exception($"Could not find adapter {converterType}");

                var adapterInstance = Activator.CreateInstance(converterType, _adapterServiceFactory);
                adapters.Add((Adapter)adapterInstance);
            }

            return components;
        }
    }

    
}
