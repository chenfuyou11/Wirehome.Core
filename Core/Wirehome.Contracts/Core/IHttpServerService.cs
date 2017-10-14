using Wirehome.Contracts.Api;
using System;
using HTTPnet.Core.Pipeline;

namespace Wirehome.Contracts.Core
{
    public interface IHttpServerService
    {
        event EventHandler<ApiRequestReceivedEventArgs> ApiRequestReceived;
        event EventHandler<HttpContextPipelineHandlerContext> HTTPRequestReceived;
    }
}
