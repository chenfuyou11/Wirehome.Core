using System;
using Wirehome.Contracts.Api;
using Wirehome.Contracts.Components;
using Newtonsoft.Json.Linq;

namespace Wirehome.Tests.Mockups
{
    public class TestApiAdapter : IApiAdapter
    {
        public event EventHandler<ApiRequestReceivedEventArgs> RequestReceived;
        public event EventHandler<ApiRequestReceivedEventArgs> ApiRequestReceived;

        public TestApiAdapter()
        {
            ApiRequestReceived?.Invoke(null, null);
        }

        public int NotifyStateChangedCalledCount { get; set; }

        public void NotifyStateChanged(IComponent component)
        {
            NotifyStateChangedCalledCount++;
        }

        public IApiCall Invoke(string action, JObject parameter)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (parameter == null) throw new ArgumentNullException(nameof(parameter));

            var apiCall = new ApiCall(action, parameter, null);
            RequestReceived?.Invoke(this, new ApiRequestReceivedEventArgs(apiCall));

            return apiCall;
        }
    }
}
