using Quartz.Spi;
using Quartz;
using HA4IoT.Contracts.Core;

namespace HA4IoT.Extensions.Quartz
{
    public static class IContainerQuartzExtensions
    {
        public static void RegisterQuartz(this IContainer container)
        {
            container.RegisterSingleton<IJobFactory, SimpleInjectorJobFactory>();
            container.RegisterSingleton<ISchedulerFactory, SimpleInjectorSchedulerFactory>();
            container.RegisterFactory(() => { return container.GetInstance<ISchedulerFactory>().GetScheduler().Result; });
        }
    }
}
