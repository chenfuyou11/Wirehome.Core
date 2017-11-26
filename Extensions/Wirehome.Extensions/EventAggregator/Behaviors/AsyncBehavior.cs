using System;
using System.Threading.Tasks;
using Wirehome.Extensions.Messaging.Core;

namespace Wirehome.Extensions.Core.Policies
{
    public class AsyncBehavior : IBehavior
    {
        private IAsyncCommandHandler _asyncCommandHandler;

        public int Priority => 50;

        public void SetNextNode(IAsyncCommandHandler asyncCommandHandler)
        {
            _asyncCommandHandler = asyncCommandHandler ?? throw new ArgumentNullException(nameof(asyncCommandHandler));
        }

        public Task<R> HandleAsync<T, R>(IMessageEnvelope<T> message) where R : class
        {
            return Task.Run(() => _asyncCommandHandler.HandleAsync<T, R>(message));
        }
    }

}
