using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Wirehome.Extensions.Messaging.Core;
using Wirehome.Extensions.Extensions;
using System.Linq.Expressions;

namespace Wirehome.Extensions.Core.Policies
{

    public class TimeoutBehavior : IBehavior
    {
        private IAsyncCommandHandler _asyncCommandHandler;
        private readonly TimeSpan _timeout;
        public int Priority => 30;

        public void SetNextNode(IAsyncCommandHandler asyncCommandHandler)
        {
            _asyncCommandHandler = asyncCommandHandler ?? throw new ArgumentNullException(nameof(asyncCommandHandler));
        }

        public TimeoutBehavior(TimeSpan timeout)
        {
            _timeout = timeout;
        }

        public Task<R> HandleAsync<T, R>(IMessageEnvelope<T> message) where R : class
        {
            return _asyncCommandHandler.HandleAsync<T, R>(message).WhenDone(_timeout, message.CancellationToken);
        }
    }

}
