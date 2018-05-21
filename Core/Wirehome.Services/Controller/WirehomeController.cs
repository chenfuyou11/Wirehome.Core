using System;
using System.Threading.Tasks;
using Wirehome.ComponentModel.Configuration;
using Wirehome.Core.Services.DependencyInjection;
using Wirehome.Core.Services.Logging;

namespace Wirehome.Model.Core
{
    public class WirehomeController
    {
        private readonly IContainer _container;
        private readonly ControllerOptions _options;
        private ILogger _log;
        
        public WirehomeController(ControllerOptions options)
        {
            _container = new WirehomeContainer(options);
            _options = options;
        }

        public async Task<bool> Run()
        {
            try
            {
                _container.RegisterServices();
                _log = _container.GetInstance<ILogService>().CreatePublisher(nameof(WirehomeController));

                await InitializeServices().ConfigureAwait(false);
                await InitializeFromConfig().ConfigureAwait(false);

                return true;
            }
            catch (Exception e)
            {
                _log.Error(e, "Unhanded exception while application startup");
                return false;
            }
        }

        private async Task InitializeFromConfig()
        {
            var confService = _container.GetInstance<IConfigurationService>();
            var configuration = await confService.ReadConfiguration(_options.ConfigurationPath, _options.AdapterRepository).ConfigureAwait(false);
        }

        private async Task InitializeServices()
        {
            var services = _container.GetSerives();

            while (services.Count > 0)
            {
                var service = services.Dequeue();
                try
                {
                    await service.Initialize().ConfigureAwait(false);
                }
                catch (Exception exception)
                {
                    _log.Error(exception, $"Error while starting service '{service.GetType().Name}'. " + exception.Message);
                }
            }
        }
    }
}
