using System;
using Wirehome.Contracts.Api;
using Newtonsoft.Json.Linq;

namespace Wirehome.Api.Cloud.Azure
{
    public class QueueBasedApiContext : ApiCall
    {
        public QueueBasedApiContext(string correlationId, string action, JObject parameter, string resultHash) 
            : base(action, parameter, resultHash)
        {
            if (correlationId == null) throw new ArgumentNullException(nameof(correlationId));

            CorrelationId = correlationId;
        }

        public string CorrelationId { get; }
    }
}
