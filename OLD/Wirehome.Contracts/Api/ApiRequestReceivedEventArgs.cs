using System;

namespace Wirehome.Contracts.Api
{
    public class ApiRequestReceivedEventArgs : EventArgs
    {
        public ApiRequestReceivedEventArgs(IApiCall apiCall)
        {
            ApiContext = apiCall ?? throw new ArgumentNullException(nameof(apiCall));
        }

        public bool IsHandled { get; set; }

        public IApiCall ApiContext { get; }
    }
}
