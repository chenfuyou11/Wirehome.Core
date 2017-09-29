using System;
using System.Threading.Tasks;
using Wirehome.Contracts;
using Wirehome.Contracts.Api;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Logging;
using Wirehome.Contracts.Services;

namespace Wirehome.Core
{
    public static class ContainerExtensions
    {
        public static void ExposeRegistrationsToApi(this Container container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));

            var apiService = container.GetInstance<IApiDispatcherService>();
            foreach (var registration in container.GetCurrentRegistrations())
            {
                apiService.Expose(registration.GetInstance());
            }
        }

        public static async Task StartupServices(this IContainer container, ILogger log)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            if (log == null) throw new ArgumentNullException(nameof(log));

            foreach (var service in container.GetInstances<IService>())
            {
                try
                {
                    await service.Initialize();
                }
                catch (Exception exception)
                {
                    log.Error(exception, $"Error while starting service '{service.GetType().Name}'. " + exception.Message);
                }
            }
        }
    }
}
