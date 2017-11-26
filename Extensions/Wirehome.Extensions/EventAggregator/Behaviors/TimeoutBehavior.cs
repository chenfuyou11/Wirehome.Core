using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Wirehome.Extensions.Messaging.Core;
using Wirehome.Extensions.Extensions;
using System.Linq.Expressions;

namespace Wirehome.Extensions.Core.Policies
{

    public class TimeoutBehavior : Behavior
    {
        private readonly TimeSpan _timeout;
        
        public TimeoutBehavior(TimeSpan timeout)
        {
            _timeout = timeout;
            Priority = 30;
        }

        public override Task<R> HandleAsync<T, R>(IMessageEnvelope<T> message)
        {
            return _asyncCommandHandler.HandleAsync<T, R>(message).WhenDone(_timeout, message.CancellationToken);
        }
    }

}
