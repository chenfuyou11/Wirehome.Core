using System;
using System.Threading.Tasks;
using Wirehome.Core.Services.DependencyInjection;
using Wirehome.Core.Services.Logging;

namespace Wirehome.Core.Extensions
{
    public static class ContainerExtensions
    {
        public static async Task StartupServices(this IContainer container, ILogger log)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            if (log == null) throw new ArgumentNullException(nameof(log));

            var services = container.GetSerives();

            while (services.Count > 0)
            {
                var service = services.Dequeue();
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