using HA4IoT.Contracts.Api;
using HA4IoT.Net.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace HA4IoT.Contracts.Core
{
    public interface IHttpServerService
    {
        event EventHandler<ApiRequestReceivedEventArgs> ApiRequestReceived;
        event EventHandler<HttpRequestReceivedEventArgs> HTTPRequestReceived;
    }
}
