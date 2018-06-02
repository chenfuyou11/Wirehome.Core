using AutoMapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Wirehome.ComponentModel.Adapters;
using Wirehome.ComponentModel.Adapters.Denon;
using Wirehome.ComponentModel.Commands;
using Wirehome.ComponentModel.Components;
using Wirehome.ComponentModel.Events;
using Wirehome.Core.ComponentModel.Areas;
using Wirehome.Core.ComponentModel.Configuration;
using Wirehome.Core.Extensions;
using Wirehome.Core.Services.DependencyInjection;
using Wirehome.Core.Services.Logging;
using Wirehome.Core.Services.Roslyn;
using Wirehome.Core.Utils;

namespace Wirehome.ComponentModel.Configuration
{

    public class ConfigurationService : IConfigurationService
    {

        private readonly IMapper _mapper;
        private readonly IAdapterServiceFactory _adapterServiceFactory;
        private readonly ILogger _logger;
        private readonly IResourceLocatorService _resourceLocatorService;
        private readonly IContainer _container;

        public ConfigurationService(IMapper mapper, IAdapterServiceFactory adapterServiceFactory, ILogService logService, 
            IResourceLocatorService resourceLocatorService, IContainer container)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _adapterServiceFactory = adapterServiceFactory ?? throw new ArgumentNullException(nameof(adapterServiceFactory));
            _logger = logService.CreatePublisher(nameof(ConfigurationService));
            _resourceLocatorService = resourceLocatorService;
            _container = container;
        }
        
        public WirehomeConfiguration ReadConfiguration()
        {
            var configPath = _resourceLocatorService.GetConfigurationPath();
            var adaptersRepoPath = _resourceLocatorService.GetRepositoyLocation();

            var rawConfig = File.ReadAllText(configPath);

            var result = JsonConvert.DeserializeObject<WirehomeConfigDTO>(rawConfig);

            var adapters = MapAdapters(result.Wirehome.Adapters, adaptersRepoPath);
            var components = MapComponents(result);
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

        private IList<Component> MapComponents(WirehomeConfigDTO result)
        {
            return _mapper.Map<IList<ComponentDTO>, IList<Component>>(result.Wirehome.Components);
        }

        private IList<Adapter> MapAdapters(IList<AdapterDTO> adapterConfigs, string adaptersRepoPath)
        {
            var adapters = new List<Adapter>();
            var types = new List<Type>(AssemblyHelper.GetAllInherited<Adapter>());
            
            foreach (var adapterConfig in adapterConfigs)
            {
                try
                {
                    var adapterType = types.Find(t => t.Name == adapterConfig.Type);
                    if (adapterType == null) throw new Exception($"Could not find adapter {adapterType}");

                    Mapper.Initialize(p => 
                    {
                        p.CreateMap(typeof(AdapterDTO), adapterType).ConstructUsingServiceLocator();
                        p.ShouldMapProperty = propInfo => (propInfo.CanWrite && propInfo.GetGetMethod(true).IsPublic) || propInfo.IsDefined(typeof(MapAttribute), false);

                        p.CreateMap<ComponentDTO, Component>().ConstructUsingServiceLocator();
                        p.CreateMap<AdapterReferenceDTO, AdapterReference>();
                        p.CreateMap<TriggerDTO, Trigger>();
                        p.CreateMap<CommandDTO, Command>();
                        p.CreateMap<EventDTO, Event>();
                        p.CreateMap<AreaDTO, Area>();
                        p.ConstructServicesUsing(_container.GetInstance);
                    });
                  
                    var adapter = (Adapter)Mapper.Map(adapterConfig, typeof(AdapterDTO), adapterType);

                    //var adapter = (Adapter)_mapper.Map(adapterConfig, typeof(AdapterDTO), adapterType);

                    adapters.Add(adapter);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Exception {ex.Message} while initializing adapter {adapterConfig.Type}");
                }
            }

            return adapters;
        }

        private IEnumerable<string> FindAdapterInRepository(string sourceDir, string filter = "*.dll") =>
        Directory.GetFiles(sourceDir, filter, SearchOption.AllDirectories);
    }
}