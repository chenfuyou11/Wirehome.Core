using AutoMapper;
using SimpleInjector;
using Wirehome.Core.EventAggregator;

namespace Wirehome.Core.DI
{
    //TODO change this to Container class 
    public class Registrar
    {
        public void Register(Container container)
        {
            container.RegisterSingleton<IEventAggregator, EventAggregator.EventAggregator>();
            container.RegisterSingleton(() => GetMapper(container));
        }

        private IMapper GetMapper(Container container)
        {
            var mp = container.GetInstance<MapperProvider>();
            return mp.GetMapper();
        }
    }
}
