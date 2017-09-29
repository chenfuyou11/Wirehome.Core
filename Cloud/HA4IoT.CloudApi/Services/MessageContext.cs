using System;
using System.Threading.Tasks;
using Wirehome.Contracts.Api.Cloud;

namespace Wirehome.CloudApi.Services
{
    public class MessageContext
    {
        private readonly TaskCompletionSource<CloudResponseMessage> _taskCompletionSource = new TaskCompletionSource<CloudResponseMessage>();

        public MessageContext(CloudRequestMessage requestMessage)
        {
            RequestMessage = requestMessage ?? throw new ArgumentNullException(nameof(requestMessage));
        }

        public DateTime CreatedTimestamp { get; } = DateTime.UtcNow;

        public TimeSpan Duration => DateTime.UtcNow - CreatedTimestamp;

        public Task<CloudResponseMessage> Task => _taskCompletionSource.Task;

        public CloudRequestMessage RequestMessage { get; }

        public CloudResponseMessage ResponseMessage { get; private set; }

        public bool IsDelivered { get; set; }

        public void Complete(CloudResponseMessage response)
        {
            ResponseMessage = response;

            if (response != null)
            {
                _taskCompletionSource.SetResult(response);
            }
            else
            {
                _taskCompletionSource.SetCanceled();
            }
        }
    }
}