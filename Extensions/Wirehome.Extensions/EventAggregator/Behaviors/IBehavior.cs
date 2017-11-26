using Wirehome.Extensions.Messaging.Core;

namespace Wirehome.Extensions.Core.Policies
{
    public interface IBehavior : IAsyncCommandHandler
    {
        void SetNextNode(IAsyncCommandHandler asyncCommandHandler);
        int Priority { get; }
    }
}