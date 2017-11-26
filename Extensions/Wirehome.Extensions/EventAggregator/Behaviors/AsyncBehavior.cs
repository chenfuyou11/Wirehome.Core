using System.Threading.Tasks;
using Wirehome.Extensions.Messaging.Core;

namespace Wirehome.Extensions.Core.Policies
{
    public class AsyncBehavior : Behavior
    {
        public AsyncBehavior() { Priority = 50; }
        
        public override Task<R> HandleAsync<T, R>(IMessageEnvelope<T> message)
        {
            return Task.Run(() => _asyncCommandHandler.HandleAsync<T, R>(message));
        }
    }
}
