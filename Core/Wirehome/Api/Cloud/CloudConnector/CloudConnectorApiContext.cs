using System;
using Wirehome.Contracts.Api;
using Wirehome.Contracts.Api.Cloud;

namespace Wirehome.Api.Cloud.CloudConnector
{
    public class CloudConnectorApiContext : ApiCall
    {
        public CloudConnectorApiContext(CloudRequestMessage requestMessage) 
            : base(requestMessage.Request.Action, requestMessage.Request.Parameter, requestMessage.Request.ResultHash)
        {
            if (requestMessage == null) throw new ArgumentNullException(nameof(requestMessage));

            RequestMessage = requestMessage;
        }

        public CloudRequestMessage RequestMessage { get; }
    }
}
