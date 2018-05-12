using AutoMapper;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Loader;
using Wirehome.ComponentModel.Adapters;
using Wirehome.ComponentModel.Commands;
using Wirehome.ComponentModel.Components;
using Wirehome.ComponentModel.Events;
using Wirehome.Core.ComponentModel.Areas;
using Wirehome.Core.ComponentModel.Configuration;
using Wirehome.Core.Utils;

namespace Wirehome.Core.Services.DependencyInjection
{
    public class WirehomeMappingProfile : Profile
    {
        public WirehomeMappingProfile(string adapterRepository)
        {
            ShouldMapProperty = propInfo => (propInfo.CanWrite && propInfo.GetGetMethod(true).IsPublic) || propInfo.IsDefined(typeof(MapAttribute), false);

            CreateMap<ComponentDTO, Component>().ConstructUsingServiceLocator();
            CreateMap<AdapterReferenceDTO, AdapterReference>();
            CreateMap<TriggerDTO, Trigger>();
            CreateMap<CommandDTO, Command>();
            CreateMap<EventDTO, Event>();
            CreateMap<AreaDTO, Area>();

            foreach (var assemblyPath in FindAdapterInRepository(adapterRepository))
            {
                foreach (var adapter in AssemblyHelper.GetAllInherited<Adapter>(AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath)))
                {
                    CreateMap(typeof(AdapterDTO), adapter).ConstructUsingServiceLocator();
                }
            }
        }

        private IEnumerable<string> FindAdapterInRepository(string sourceDir, string filter = "*Adapter*.dll") =>
       Directory.GetFiles(sourceDir, filter, SearchOption.AllDirectories);
    }

    public class ConfigurationPathService : IConfigurationPathService
    {
        public string GetAdapterRepositoryPath()
        {
            return "";
        }
    }
}