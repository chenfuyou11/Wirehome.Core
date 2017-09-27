using System;
using Wirehome.Contracts.Network.Http;

namespace Wirehome.Contracts.Network.Websockets
{
    public class UpgradedToWebSocketSessionEventArgs : EventArgs
    {
        public UpgradedToWebSocketSessionEventArgs(HttpRequest httpRequest)
        {
            HttpRequest = httpRequest ?? throw new ArgumentNullException(nameof(httpRequest));
        }

        public HttpRequest HttpRequest { get; }
    }
}
