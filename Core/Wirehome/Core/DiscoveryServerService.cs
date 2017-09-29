using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Wirehome.Contracts.Settings;
using Wirehome.Contracts.Services;
using Wirehome.Contracts.Core;
using Wirehome.Settings;
using Wirehome.Contracts.Core.Discovery;

namespace Wirehome.Core
{
    public sealed class DiscoveryServerService : ServiceBase, IDisposable
    {
        private const int Port = 19228;

        private readonly ISettingsService _settingsService;
        private readonly INativeUDPSocket _nativeUDPSocket;

        public DiscoveryServerService(ISettingsService settingsService, INativeUDPSocket nativeUDPSocket)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _nativeUDPSocket = nativeUDPSocket ?? throw new ArgumentNullException(nameof(nativeUDPSocket));
        }

        public override async Task Initialize()
        {
            _nativeUDPSocket.OnMessageRecived += _nativeUDPSocket_OnMessageRecived;
            await _nativeUDPSocket.BindServiceNameAsync(Port).ConfigureAwait(false);
        }

        private async void _nativeUDPSocket_OnMessageRecived(string remoteAddress)
        {
            await SendResponseAsync(remoteAddress);
        }

        public void Dispose()
        {
            _nativeUDPSocket?.Dispose();
        }

        private async Task SendResponseAsync(string remoteAddress)
        {
            //TODO Why not in constructor??
            var controllerSettings = _settingsService.GetSettings<ControllerSettings>();

            var response = new DiscoveryResponse(controllerSettings.Caption, controllerSettings.Description);
            var buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response));

            await _nativeUDPSocket.SendResponse(remoteAddress, Port, buffer).ConfigureAwait(false);
        }

   
    }
}
