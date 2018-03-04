using Quartz.Spi;
using Quartz;

namespace Wirehome.Core.Services.Quartz
{
    public static class IContainerQuartzExtensions
    {
        public static void RegisterQuartz(this IContainer container)
        {
            container.RegisterSingleton<IJobFactory, SimpleInjectorJobFactory>();
            container.RegisterSingleton<ISchedulerFactory, SimpleInjectorSchedulerFactory>();
            container.RegisterFactory(() => container.GetInstance<ISchedulerFactory>().GetScheduler().Result);
        }
    }
}
