using AutoMapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wirehome.ComponentModel.Adapters;
using Wirehome.ComponentModel.Components;
using Wirehome.Core.ComponentModel.Areas;
using Wirehome.Core.ComponentModel.Configuration;
using Wirehome.Core.Utils;
using Wirehome.Core.Extensions;
using System.IO;
using System.Runtime.Loader;
using Wirehome.Core.Services.Logging;

namespace Wirehome.ComponentModel.Configuration
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly IMapper _mapper;
        private readonly IAdapterServiceFactory _adapterServiceFactory;
        private readonly ILogger _logger;

        public ConfigurationService(IMapper mapper, IAdapterServiceFactory adapterServiceFactory, ILogService logService)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _adapterServiceFactory = adapterServiceFactory ?? throw new ArgumentNullException(nameof(adapterServiceFactory));
            _logger = logService.CreatePublisher(nameof(ConfigurationService));
        }

        public async Task<WirehomeConfiguration> ReadConfiguration(string rawConfig, string adaptersRepoPath)
        {
            var result = JsonConvert.DeserializeObject<WirehomeConfigDTO>(rawConfig);

            var adapters = await MapAdapters(result.Wirehome.Adapters, adaptersRepoPath);
            var components = await MapComponents(result);
            var areas = MapAreas(result, components);

            var configuration = new WirehomeConfiguration
            {
                Adapters = adapters,
                Components = components,
                Areas = areas
            };

            CheckForDuplicateUid(configuration);

            return configuration;
        }

        private void CheckForDuplicateUid(WirehomeConfiguration configuration)
        {
            var allUids = configuration.Adapters.Select(a => a.Uid).ToList();
            allUids.AddRange(configuration.Components.Select(c => c.Uid));

            var xx = configuration.Areas.Expand(a => a.Areas);

            var duplicateKeys = allUids.GroupBy(x => x)
                                       .Where(group => group.Count() > 1)
                                       .Select(group => group.Key);
            if (duplicateKeys?.Count() > 0)
            {
                throw new Exception($"Duplicate UID's found in config file: {string.Join(", ", duplicateKeys)}");
            }
        }

        private IList<Area> MapAreas(WirehomeConfigDTO result, IList<Component> components)
        {
            var areas = _mapper.Map<IList<AreaDTO>, IList<Area>>(result.Wirehome.Areas);
            MapComponentsToArea(result.Wirehome.Areas, components, areas);

            return areas;
        }

        private void MapComponentsToArea(IList<AreaDTO> areasFromConfig, IList<Component> components, IList<Area> areas)
        {
            var configAreas = areasFromConfig.Expand(a => a.Areas);
            foreach (var area in areas.Expand(a => a.Areas))
            {
                var areInConfig = configAreas.FirstOrDefault(a => a.Uid == area.Uid);
                if (areInConfig?.Components != null)
                {
                    foreach (var component in areInConfig?.Components)
                    {
                        area.AddComponent(components.FirstOrDefault(c => c.Uid == component.Uid));
                    }
                }
            }
        }

        private async Task<IList<Component>> MapComponents(WirehomeConfigDTO result)
        {
            var components = _mapper.Map<IList<ComponentDTO>, IList<Component>>(result.Wirehome.Components);
            foreach (var component in components)
            {
                await component.Initialize();
            }
            return components;
        }

        private async Task<IList<Adapter>> MapAdapters(IList<AdapterDTO> adapterConfigs, string adaptersRepoPath)
        {
            var adapters = new List<Adapter>();
            var types = new List<Type>();

            foreach (var assemblyPath in FindAdapterInRepository(adaptersRepoPath))
            {
                var adapterTypes = AssemblyHelper.GetAllInherited<Adapter>(AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath));
                
                types.AddRange(adapterTypes);
            }

            foreach (var adapterConfig in adapterConfigs)
            {
                try
                {
                    var adapterType = types.Find(t => t.Name == adapterConfig.Type);
                    if (adapterType == null) throw new Exception($"Could not find adapter {adapterType}");
                    var adapter = (Adapter)_mapper.Map(adapterConfig, typeof(AdapterDTO), adapterType);

                    await adapter.Initialize();
                    adapters.Add(adapter);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Exception {ex.Message} while initializing adpater {adapterConfig.Type}");
                }
            }

            return adapters;
        }

        private IEnumerable<string> FindAdapterInRepository(string sourceDir, string filter = "*Adapter*.dll") =>
        Directory.GetFiles(sourceDir, filter, SearchOption.AllDirectories);
    }
}