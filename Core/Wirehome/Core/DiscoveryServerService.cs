using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Wirehome.Contracts.Settings;
using Wirehome.Contracts.Services;
using Wirehome.Settings;
using Wirehome.Contracts.Core.Discovery;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace Wirehome.Core
{
    //TODO Test after migrate to .NET Standard 2.0
    public sealed class DiscoveryServerService : ServiceBase, IDisposable
    {
        private const int Port = 19228;
        private readonly CancellationTokenSource _cancelationToken = new CancellationTokenSource();
        private readonly ISettingsService _settingsService;
        private readonly UdpClient _socket;
        
        public DiscoveryServerService(ISettingsService settingsService)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _socket = new UdpClient(Port);
        }

        public override Task Initialize()
        {
            Task.Run(async () =>
            {
                var token = _cancelationToken.Token;
                while (true)
                {
                    if (_cancelationToken.IsCancellationRequested) break;
                    var result = await _socket.ReceiveAsync().ConfigureAwait(false);
                    await SendResponseAsync(result.RemoteEndPoint).ConfigureAwait(false);
                }
            });

            return Task.CompletedTask;
        }
        
        public void Dispose()
        {
            _cancelationToken.Cancel();
            _socket.Dispose();
        }

        private async Task SendResponseAsync(IPEndPoint remoteAddress )
        {
            //TODO Why not in constructor??
            var controllerSettings = _settingsService.GetSettings<ControllerSettings>();

            var response = new DiscoveryResponse(controllerSettings.Caption, controllerSettings.Description);
            var buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response));

            await _socket.SendAsync(buffer, buffer.Length, remoteAddress);
        }

   
    }
}
