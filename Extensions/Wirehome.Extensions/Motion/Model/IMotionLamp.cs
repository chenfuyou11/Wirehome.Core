using System;
using Wirehome.Contracts.Components.Commands;

namespace Wirehome.Motion.Model
{
    public interface IMotionLamp
    {
        string Id { get; }
        void ExecuteCommand(ICommand command);
        IObservable<PowerStateChangeEvent> PowerStateChange { get; }
    }
}