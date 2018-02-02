using System;
using Wirehome.Contracts.Components.Commands;

namespace Wirehome.Motion.Model
{
    public interface IMotionLamp
    {
        string Id { get; }

        bool GetIsTurnedOn();

        IObservable<PowerStateChangeEvent> PowerStateChange { get; }

        void ExecuteCommand(ICommand command);
        string ToString();
    }
}