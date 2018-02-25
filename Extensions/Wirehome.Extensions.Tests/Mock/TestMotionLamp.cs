using System;
using Wirehome.Contracts.Components.Commands;
using System.Reactive.Subjects;
using Wirehome.Contracts.Components.States;

namespace Wirehome.Motion.Model
{
    public class TestMotionLamp : IMotionLamp
    {
        private readonly Subject<PowerStateChangeEvent> _powerStateSubject = new Subject<PowerStateChangeEvent>();
        private IObservable<PowerStateChangeEvent> _powerStateChange;
        private bool isTurnedOn;

        public TestMotionLamp(string id)
        {
            Id = id;
        }

        public string Id { get; }
        public bool GetIsTurnedOn() => isTurnedOn;
        
        private void SetIsTurnedOn(bool value)
        {
            if (value != isTurnedOn && _powerStateSubject != null)
            {
                var powerStateValue = value ? PowerStateValue.On : PowerStateValue.Off;
                _powerStateSubject.OnNext(new PowerStateChangeEvent(powerStateValue, PowerStateChangeEvent.AutoSource));
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

        public void SetPowerStateSource(IObservable<PowerStateChangeEvent> source) => PowerStateChange = source;
        public override string ToString() => $"{Id} : {GetIsTurnedOn()}";

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
        
    }

}