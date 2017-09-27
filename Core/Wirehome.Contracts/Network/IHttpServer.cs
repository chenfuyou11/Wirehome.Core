using System;
using System.Threading.Tasks;
using Wirehome.Contracts.Network.Websockets;

namespace Wirehome.Contracts.Network.Http
{
    public interface IHttpServer
    {
        event EventHandler<HttpRequestReceivedEventArgs> HttpRequestReceived;
        event EventHandler<WebSocketConnectedEventArgs> WebSocketConnected;

        Task BindAsync(int port);
        void Dispose();
    }
}