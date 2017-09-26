using System;

namespace Wirehome.Net.Http
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
