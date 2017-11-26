using System;
using System.Threading;
using System.Threading.Tasks;
using Wirehome.Extensions.Core.Policies;

namespace Wirehome.Extensions.Messaging.Core.Extensions
{
    public static class EventAggregateExtensions
    {
       public static Task<R> QueryAsync<T, R>
       (
           this IEventAggregator eventAggregate,
           T message,
           MessageFilter filter = null,
           CancellationToken cancellationToken = default,
           TimeSpan? timeout = null,
           int retryCount = 0,
           bool async = false
       ) where R : class
        {
            var chain = new BehaviorChain().WithTimeout(timeout).WithRetry(retryCount).WithAsync(async);
            return eventAggregate.QueryAsync<T, R>(message, filter, cancellationToken, chain);
        }
    }
}
