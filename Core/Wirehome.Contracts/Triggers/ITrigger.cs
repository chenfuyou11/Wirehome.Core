using System;
using Wirehome.Contracts.Core;

namespace Wirehome.Contracts.Triggers
{
    public interface ITrigger
    {
        event EventHandler<TriggeredEventArgs> Triggered;

        bool IsAnyAttached { get; }

        void Attach(Action action);

        void Attach(IAction action);
    }
}
