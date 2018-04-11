using Wirehome.Contracts.Components;
using System;

namespace Wirehome.Contracts.Api
{
    public interface IApiAdapter
    {
        event EventHandler<ApiRequestReceivedEventArgs> ApiRequestReceived;

        void NotifyStateChanged(IComponent component);
    }
}
