using System;
using Wirehome.Contracts.Automations;

namespace Wirehome.Automations
{
    public abstract class AutomationBase : IAutomation
    {
        protected AutomationBase(string id)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
        }

        public string Id { get; }
    }
}
