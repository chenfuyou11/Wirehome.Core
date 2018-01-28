using System;
using Wirehome.Contracts.Components.Commands;

namespace Wirehome.Motion
{
    public interface IMotionLamp
    {
        string Id { get; }
        bool IsTurnedOn { get; }
        IObservable<PowerStateChangeEvent> PowerStateChange { get; }

        void ExecuteCommand(ICommand command);
        string ToString();
    }
}