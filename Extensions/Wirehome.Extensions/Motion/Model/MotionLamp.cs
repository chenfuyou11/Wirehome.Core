using System;
using Wirehome.Contracts.Components.Commands;
using System.Reactive.Subjects;
using Wirehome.Contracts.Components.States;

namespace Wirehome.Motion.Model
{
    public class MotionLamp : IMotionLamp
    {
        private Subject<PowerStateChangeEvent> _powerStateSubject = new Subject<PowerStateChangeEvent>();
        private IObservable<PowerStateChangeEvent> _powerStateChange;

        public MotionLamp(string id)
        {
            Id = id;
        }

        public string Id { get; }

        private bool isTurnedOn;

        public bool GetIsTurnedOn()
        {
            return isTurnedOn;
        }

        private void SetIsTurnedOn(bool value)
        {
            if(value != isTurnedOn)
            {
                
                if(_powerStateSubject != null)
                {
                    var powerStateValue = value ? PowerStateValue.On : PowerStateValue.Off;
                    _powerStateSubject.OnNext(new PowerStateChangeEvent(powerStateValue, PowerStateChangeEvent.AutoSource));
                }
            }
            isTurnedOn = value;
        }

        public IObservable<PowerStateChangeEvent> PowerStateChange
        {
            get
            {
                return _powerStateChange ?? _powerStateSubject;
            }
            private set
            {
                _powerStateChange = value;
            }
        }

        public void SetPowerStateSource(IObservable<PowerStateChangeEvent> source)
        {
            PowerStateChange = source;
        }

        public void ExecuteCommand(ICommand command)
        {
            if (command is TurnOnCommand)
            {
                SetIsTurnedOn(true);
            }
            else if (command is TurnOffCommand)
            {
                SetIsTurnedOn(false);
            }
            else
            {
                throw new NotSupportedException($"Not supported command {command}");
            }
        }
        
        public override string ToString() => $"{Id} : {GetIsTurnedOn()}";

    }

}