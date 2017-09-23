using System;
using System.Text;
using System.Threading.Tasks;
using HA4IoT.Contracts.Core.Discovery;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Settings;
using HA4IoT.Settings;
using Newtonsoft.Json;
using SocketLite.Services;
using SocketLite.Model;
using System.Linq;

namespace HA4IoT.Core
{
    public sealed class DiscoveryServerService : ServiceBase, IDisposable
    {
        private const int Port = 19228;
        private UdpSocketReceiver _socket = new UdpSocketReceiver();
        private IDisposable _subscriberUpdReceiver;
        private readonly ISettingsService _settingsService;

        public DiscoveryServerService(ISettingsService settingsService)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        }

        public override void Startup()
        {
            var allInterfaces = new CommunicationsInterface().GetAllInterfaces();
            var observerUdpReceiver =_socket.CreateObservableListener(Port, allInterfaces.FirstOrDefault());

            _subscriberUpdReceiver = observerUdpReceiver.Subscribe(
            async args =>
            {
                var controllerSettings = _settingsService.GetSettings<ControllerSettings>();
                var response = new DiscoveryResponse(controllerSettings.Caption, controllerSettings.Description);
                await SendResponseAsync(args.RemoteAddress, response);
            });
        }

        public void Dispose()
        {
            _subscriberUpdReceiver.Dispose();
            _socket?.Dispose();
        }
        
        private static async Task SendResponseAsync(string target, DiscoveryResponse response)
        {
            var buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response));
            using (var socket = new UdpSocketClient())
            {
                await socket.ConnectAsync(target, Port);
                await socket.SendAsync(buffer);
            }
        }
    }
}
