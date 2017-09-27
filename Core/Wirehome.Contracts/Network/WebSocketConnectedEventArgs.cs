using System;
using Wirehome.Contracts.Network.Http;

namespace Wirehome.Contracts.Network.Websockets
{
    public class WebSocketConnectedEventArgs : EventArgs
    {
        public WebSocketConnectedEventArgs(HttpRequest httpRequest, IWebSocketClientSession webSocketClientSession)
        {
            if (httpRequest == null) throw new ArgumentNullException(nameof(httpRequest));
            if (webSocketClientSession == null) throw new ArgumentNullException(nameof(webSocketClientSession));

            HttpRequest = httpRequest;
            WebSocketClientSession = webSocketClientSession;
        }

        public HttpRequest HttpRequest { get; set; }

        public IWebSocketClientSession WebSocketClientSession { get; }

        public bool IsHandled { get; set; }
    }
}
