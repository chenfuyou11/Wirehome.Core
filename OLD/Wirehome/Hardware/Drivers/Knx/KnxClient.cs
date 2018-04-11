using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Logging;

namespace Wirehome.Hardware.Drivers.Knx
{
    //TODO Test after migrate to .NET Standard
    public sealed class KnxClient : IDisposable
    {
        private readonly int _port;
        private readonly string _password;
        private readonly string _hostName;
        private bool _isConnected;
        private bool _isDisposed;
        private readonly TcpClient _socket;
        private NetworkStream _stream;

        public KnxClient(string hostName, int port, string password)
        {
            _hostName = hostName ?? throw new ArgumentNullException(nameof(hostName));
            _port = port;
            _password = password;
;
            _socket = new TcpClient();

            //TODO??
            //_socket.Control.KeepAlive = true;
            //_socket.Control.NoDelay = true;
        }

        public int Timeout { get; set; } = 150;

        public async Task Connect()
        {
            ThrowIfDisposed();

            Log.Default.Verbose($"KnxClient: Connecting with {_hostName}...");

            await _socket.ConnectAsync(_hostName, _port).ConfigureAwait(false);
            
            _isConnected = true;

            await Authenticate().ConfigureAwait(false);

            Log.Default.Verbose("KnxClient: Connected");
        }

        public async Task SendRequest(string request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            ThrowIfDisposed();
            ThrowIfNotConnected();

            await WriteToSocket(request).ConfigureAwait(false);
        }

        public async Task<string> SendRequestAndWaitForResponse(string request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            ThrowIfDisposed();
            ThrowIfNotConnected();

            await WriteToSocket(request).ConfigureAwait(false);
            return await ReadFromSocket().ConfigureAwait(false);
        }

        private async Task Authenticate()
        {
            Log.Default.Verbose("KnxClient: Authenticating...");
            string response = await SendRequestAndWaitForResponse($"p={_password}").ConfigureAwait(false);

            ThrowIfNotAuthenticated(response);
        }

        private void ThrowIfNotAuthenticated(string response)
        {
            if (!response.Equals("p=ok\x03"))
            {
                throw new InvalidOperationException("Invalid password specified for KNX client.");
            }
        }

        private void ThrowIfNotConnected()
        {
            if (!_isConnected)
            {
                throw new InvalidOperationException("The KNX Client is not connected.");
            }
        }

        private async Task WriteToSocket(string request)
        {
            byte[] payload = Encoding.UTF8.GetBytes(request + "\x03");
            _stream = _socket.GetStream();
            _stream.WriteTimeout = Timeout;

            await _stream.WriteAsync(payload, 0, payload.Length).ConfigureAwait(false);
            await _stream.FlushAsync().ConfigureAwait(false);
            
            Log.Default.Verbose($"KnxClient: Sent {request}");
        }

        private async Task<string> ReadFromSocket()
        {
            Log.Default.Verbose("KnxClient: Waiting for response...");
            
            var buffer = new byte[64];
            await _stream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
            
            var response = Encoding.UTF8.GetString(buffer);
            Log.Default.Verbose($"KnxClient: Received {response}");

            return response;
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed) throw new InvalidOperationException("The KNX client is already disposed.");
        }

        public void Dispose()
        {
            _isDisposed = true;
            _stream.Dispose();
            _socket.Dispose();
        }
    }
}