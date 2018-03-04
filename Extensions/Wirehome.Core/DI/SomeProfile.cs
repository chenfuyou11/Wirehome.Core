using AutoMapper;
using Wirehome.ComponentModel.Components;
using Wirehome.Core.ComponentModel.Configuration;

namespace Wirehome.Core.DI
{
    public class SomeProfile : Profile
    {
        public SomeProfile()
        {
            ShouldMapProperty = propInfo => (propInfo.CanWrite && propInfo.GetGetMethod(true).IsPublic) || propInfo.IsDefined(typeof(MapAttribute), false);
            CreateMap<ComponentDTO, Component>().ConstructUsingServiceLocator();
            CreateMap<AdapterReferenceDTO, AdapterReference>().ForMember(m => m.Tags, p => p.Ignore())
                                                              .ForMember(m => m.Type, p => p.Ignore());
        }
    }


}
