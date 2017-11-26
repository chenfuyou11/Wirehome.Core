using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Wirehome.Extensions.Messaging.Core;
using Wirehome.Extensions.Extensions;
using System.Linq.Expressions;

namespace Wirehome.Extensions.Core.Policies
{

    public class RetryBehavior : IBehavior
    {
        private IAsyncCommandHandler _asyncCommandHandler;
        private int _retryCount;
        public int Priority => 40;

        public void SetNextNode(IAsyncCommandHandler asyncCommandHandler)
        {
            _asyncCommandHandler = asyncCommandHandler ?? throw new ArgumentNullException(nameof(asyncCommandHandler));
        }

        public RetryBehavior(int retryCount = 3)
        {
            _retryCount = retryCount;
        }

        public async Task<R> HandleAsync<T, R>(IMessageEnvelope<T> message) where R : class
        {
            while (true)
            {
                try
                {
                    return await _asyncCommandHandler.HandleAsync<T, R>(message).ConfigureAwait(false);
                }
                catch when (_retryCount-- > 0) { }
            }
        }

        
    }

}
