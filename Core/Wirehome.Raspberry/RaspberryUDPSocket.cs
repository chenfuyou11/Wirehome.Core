using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Wirehome.Contracts.Core;

namespace Wirehome.Raspberry
{
    public class RaspberryUDPSocket : INativeUDPSocket
    {
        private readonly DatagramSocket _socket = new DatagramSocket();
        public event Action<string> OnMessageRecived;

        public RaspberryUDPSocket()
        {
            _socket.MessageReceived += _socket_MessageReceived;
        }

        private void _socket_MessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            //TODO check RemoteAddress.ToString()
            OnMessageRecived?.Invoke(args.RemoteAddress.ToString());
        }

        public async Task BindServiceNameAsync(int port)
        {
            await _socket.BindServiceNameAsync(port.ToString());
        }

        public async Task SendResponse(string address, int port, byte[] buffer)
        {
            using (var socket = new DatagramSocket())
            {
                await socket.ConnectAsync(new Windows.Networking.HostName(address), port.ToString());
                await socket.OutputStream.WriteAsync(buffer.AsBuffer());
                await socket.OutputStream.FlushAsync();
            }
        }

        public void Dispose()
        {
            _socket.Dispose();
        }
    }
}
