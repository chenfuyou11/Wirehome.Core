using AutoMapper;
using AutoMapper.Configuration;
using SimpleInjector;
using System.Linq;

namespace Wirehome.Core.DI
{
    public class MapperProvider
    {
        private readonly Container _container;

        public MapperProvider(Container container)
        {
            _container = container;
        }

        public IMapper GetMapper()
        {
            var mce = new MapperConfigurationExpression();
            mce.ConstructServicesUsing(_container.GetInstance);

            var profiles = typeof(WirehomeMappingProfile).Assembly
                                                         .GetTypes()
                                                         .Where(t => typeof(Profile).IsAssignableFrom(t))
                                                         .ToList();

            mce.AddProfiles(profiles);

            var mc = new MapperConfiguration(mce);
            mc.AssertConfigurationIsValid();

            return new Mapper(mc, t => _container.GetInstance(t));
        }
    }


}
