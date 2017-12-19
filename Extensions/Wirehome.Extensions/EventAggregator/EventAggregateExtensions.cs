using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Wirehome.Contracts.Core;
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


        public static void RegisterHandlers(this IEventAggregator eventAggregator, IContainer container)
        {
            foreach (var type in container.GetRegisterTypes().Where(x => x.GetInterfaces().Any(y => y.IsGenericType && y.GetGenericTypeDefinition() == typeof(IHandler<>))))
            {
                var handler = container.GetInstance(type);

                foreach (var handlerInterface in type.GetInterfaces()
                                                     .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IHandler<>) && x.GenericTypeArguments.Length == 1))
                {
                    var messageType = handlerInterface.GenericTypeArguments.FirstOrDefault();

                    var methodInfo = handlerInterface.GetMethods().FirstOrDefault();
                    var delegateType = Expression.GetActionType(methodInfo.GetParameters().Select(p => p.ParameterType).ToArray());
                    var @delegate = Delegate.CreateDelegate(delegateType, handler, methodInfo.Name);

                    eventAggregator.Subscribe(messageType, @delegate);
                }
            }
        }
    }
}
