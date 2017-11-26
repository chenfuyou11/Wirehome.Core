using System.Collections.Generic;
using System.Linq;
using Wirehome.Extensions.Messaging.Core;

namespace Wirehome.Extensions.Core.Policies
{
    public class BehaviorChain
    {
        private readonly List<IBehavior> _policies = new List<IBehavior>();

        public BehaviorChain WithPolicy(IBehavior policy)
        {
            _policies.Add(policy);
            return this;
        }

        public IAsyncCommandHandler Build(IAsyncCommandHandler handler, bool orderByPriority = true)
        {
            var nextHandler = handler;
            
            foreach (var policy in _policies.OrderBy(x => orderByPriority ? x.Priority : -1))
            {
                policy.SetNextNode(nextHandler);
                nextHandler = policy;
            }

            return nextHandler;
        }
    }

}
