using Wirehome.Contracts.Api;
using Wirehome.Net.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Wirehome.Contracts.Core
{
    public interface IHttpServerService
    {
        event EventHandler<ApiRequestReceivedEventArgs> ApiRequestReceived;
        event EventHandler<HttpRequestReceivedEventArgs> HTTPRequestReceived;
    }
}
