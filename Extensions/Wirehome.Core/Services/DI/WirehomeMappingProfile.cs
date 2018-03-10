using AutoMapper;
using Wirehome.ComponentModel.Adapters;
using Wirehome.ComponentModel.Commands;
using Wirehome.ComponentModel.Components;
using Wirehome.ComponentModel.Events;
using Wirehome.Core.ComponentModel.Configuration;

namespace Wirehome.Core.Services.DependencyInjection
{
    public class WirehomeMappingProfile : Profile
    {
        public WirehomeMappingProfile()
        {
            ShouldMapProperty = propInfo => (propInfo.CanWrite && propInfo.GetGetMethod(true).IsPublic) || propInfo.IsDefined(typeof(MapAttribute), false);

            CreateMap<ComponentDTO, Component>().ConstructUsingServiceLocator();
            CreateMap<AdapterReferenceDTO, AdapterReference>();
            CreateMap<TriggerDTO, Trigger>();
            CreateMap<CommandDTO, Command>();
            CreateMap<EventDTO, Event>();
            CreateMap<AdapterDTO, Adapter>();
        }
    }
}
