using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Wirehome.Contracts.Core;

namespace Wirehome.Raspberry
{
    public class RaspberryTCPSocket : INativeTCPSocket
    {
        private readonly StreamSocket _socket;

        public RaspberryTCPSocket(StreamSocket socket)
        {
            _socket = socket;

            _socket.Control.KeepAlive = true;
            _socket.Control.NoDelay = true;
        }

        public async Task ConnectAsync(string hostName, int port, int timeout)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(timeout);

            try
            {
                await _socket.ConnectAsync(new HostName(hostName), port.ToString()).AsTask(cts.Token).ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                throw new TimeoutException("Timeout while connecting KNX Client.");
            }
        }

        public async Task SendDataAsync(byte[] data, int timeout, bool autoflush)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(timeout);

            try
            {
                await _socket.OutputStream.WriteAsync(data.AsBuffer()).AsTask(cts.Token).ConfigureAwait(false);
                if(autoflush)
                {
                    await _socket.OutputStream.FlushAsync();
                }
            }
            catch (TaskCanceledException)
            {
                throw new TimeoutException("Timeout while sending KNX Client request.");
            }
        }

        public async Task ReadDataAsync(byte[] data, int timeout)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(timeout);

            try
            {
                var buffer = data.AsBuffer();
                await _socket.InputStream.ReadAsync(buffer, buffer.Capacity, InputStreamOptions.Partial).AsTask(cts.Token).ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                throw new TimeoutException("Timeout while reading KNX Client response.");
            }
        }

        public async Task<string> ReadLineAsync()
        {
            using (var reader = new StreamReader(_socket.InputStream.AsStreamForRead()))
            {
                return await reader.ReadLineAsync().ConfigureAwait(false);
            }
        }

        public void Dispose()
        {
            _socket.Dispose();
        }
    }
}
