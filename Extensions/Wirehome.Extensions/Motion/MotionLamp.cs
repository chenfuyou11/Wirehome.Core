using System;
using Wirehome.Contracts.Components.Commands;
using Wirehome.Contracts.Components;

namespace Wirehome.Motion
{
    public class MotionLamp : IComponent
    {
        public MotionLamp(string id)
        {
            Id = id;
        }

        public string Id { get; }

        public event EventHandler<ComponentFeatureStateChangedEventArgs> StateChanged;

        public bool IsTurnedOn { get; private set; }

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

        public IComponentFeatureCollection GetFeatures()
        {
            throw new NotImplementedException();
        }

        public IComponentFeatureStateCollection GetState()
        {
            throw new NotImplementedException();
        }

        public override string ToString() => $"{Id} : {IsTurnedOn}";

    }

}