using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wirehome.Extensions.Messaging.Core
{
    public interface IAsyncCommandHandler
    {
        Task<R> HandleAsync<T, R>(IMessageEnvelope<T> message) where R : class;
    }
}