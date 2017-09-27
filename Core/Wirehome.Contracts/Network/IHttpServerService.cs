using Wirehome.Contracts.Api;
using System;
using Wirehome.Contracts.Network.Http;

namespace Wirehome.Contracts.Core
{
    public interface IHttpServerService
    {
        event EventHandler<ApiRequestReceivedEventArgs> ApiRequestReceived;
        event EventHandler<HttpRequestReceivedEventArgs> HTTPRequestReceived;
    }
}
