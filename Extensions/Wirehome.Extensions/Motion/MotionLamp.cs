using System;
using Wirehome.Contracts.Components.Commands;
using Wirehome.Contracts.Components;

namespace Wirehome.Motion
{
    public class MotionLamp : IMotionLamp
    {
        public MotionLamp(string id)
        {
            Id = id;
        }

        public string Id { get; }
        
        public bool IsTurnedOn { get; private set; }

        public IObservable<PowerStateChangeEvent> PowerStateChange { get; private set; }

        public void SetPowerStateSource(IObservable<PowerStateChangeEvent> source)
        {
            PowerStateChange = source;
        }

        public void ExecuteCommand(ICommand command)
        {
            if (command is TurnOnCommand)
            {
                IsTurnedOn = true;
            }
            else if (command is TurnOffCommand)
            {
                IsTurnedOn = false;
            }
            else
            {
                throw new NotSupportedException($"Not supported command {command}");
            }
        }
        
        public override string ToString() => $"{Id} : {IsTurnedOn}";

    }

}