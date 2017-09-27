using System;
using Wirehome.Contracts.Network.Websockets;

namespace Wirehome.HttpServer.WebSockets
{
    public class WebSocketBinaryMessage : WebSocketMessage
    {
        public WebSocketBinaryMessage(byte[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            Data = data;
        }

        public byte[] Data { get; }
    }
}
