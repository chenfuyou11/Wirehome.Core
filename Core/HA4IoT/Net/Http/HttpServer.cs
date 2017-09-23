using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using HA4IoT.Contracts.Logging;
using HA4IoT.Net.WebSockets;
using SocketLite.Services;
using SocketLite.Model;
using System.Linq;

namespace HA4IoT.Net.Http
{
    //TODODNF
    public sealed class HttpServer : IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        //private readonly StreamSocketListener _serverSocket = new StreamSocketListener();
        private readonly TcpSocketListener _serverSocket = new TcpSocketListener();

        private readonly ILogger _log;

        public HttpServer(ILogService logService)
        {
            _log = logService?.CreatePublisher(nameof(HttpServer)) ?? throw new ArgumentNullException(nameof(logService));

            //_serverSocket.Control.KeepAlive = true;
            //_serverSocket.Control.NoDelay = true;
            //_serverSocket.ConnectionReceived += HandleConnection;
        }

        public event EventHandler<HttpRequestReceivedEventArgs> HttpRequestReceived;
        public event EventHandler<WebSocketConnectedEventArgs> WebSocketConnected;

        public async Task BindAsync(int port)
        {
            var communicationInterface = new CommunicationsInterface();
            var allInterfaces = communicationInterface.GetAllInterfaces();
            //var networkInterface = allInterfaces.FirstOrDefault(x => x.IpAddress == "192.168.0.2");

            var observerTcpListner = await _serverSocket.CreateObservableListener(port, allInterfaces.FirstOrDefault());
            //await _serverSocket.BindServiceNameAsync(port.ToString(), SocketProtectionLevel.PlainSocket);
            _log.Info($"Binded HTTP server to port {port}");

            var subscriberTcpListener = observerTcpListner.Subscribe(
            tcpClient =>
            {
                //using (var clientSession = new ClientSession(clientSocket, _log))
                //{
                //    clientSession.HttpRequestReceived += HandleHttpRequest;
                //    clientSession.WebSocketConnected += HandleWebSocketConnected;

                //    try
                //    {
                //        await clientSession.RunAsync();
                //    }
                //    catch (Exception exception)
                //    {
                //        var comException = exception as COMException;
                //        if (comException?.HResult == -2147014843)
                //        {
                //            return;
                //        }

                //        _log.Verbose("Error while handling HTTP client requests. " + exception);
                //    }
                //    finally
                //    {
                //        clientSession.HttpRequestReceived -= HandleHttpRequest;
                //        clientSession.WebSocketConnected -= WebSocketConnected;
                //    }
                //}
            },
            ex =>
            {
                // Insert your exception code here
            },
            () =>
            {
                // Insert your completed code here
            });
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _serverSocket.Dispose();
        }

        //private void HandleConnection(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        //{
        //    Task.Run(() => HandleConnectionAsync(args.Socket), _cancellationTokenSource.Token);
        //}

        //private async Task HandleConnectionAsync(StreamSocket clientSocket)
        //{
        //    using (var clientSession = new ClientSession(clientSocket, _log))
        //    {
        //        clientSession.HttpRequestReceived += HandleHttpRequest;
        //        clientSession.WebSocketConnected += HandleWebSocketConnected;

        //        try
        //        {
        //            await clientSession.RunAsync();
        //        }
        //        catch (Exception exception)
        //        {
        //            var comException = exception as COMException;
        //            if (comException?.HResult == -2147014843)
        //            {
        //                return;
        //            }

        //            _log.Verbose("Error while handling HTTP client requests. " + exception);
        //        }
        //        finally
        //        {
        //            clientSession.HttpRequestReceived -= HandleHttpRequest;
        //            clientSession.WebSocketConnected -= WebSocketConnected;
        //        }
        //    }
        //}

        private void HandleWebSocketConnected(object sender, WebSocketConnectedEventArgs eventArgs)
        {
            WebSocketConnected?.Invoke(this, eventArgs);
        }

        private void HandleHttpRequest(object sender, HttpRequestReceivedEventArgs eventArgs)
        {
            var handlerCollection = HttpRequestReceived;
            if (handlerCollection == null)
            {
                return;
            }

            foreach (var handler in handlerCollection.GetInvocationList())
            {
                handler.DynamicInvoke(this, eventArgs);
                if (eventArgs.IsHandled)
                {
                    return;
                }
            }
        }
    }
}