using System;
using HA4IoT.Actuators.StateMachines;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Services.System;
using System.Diagnostics;

namespace HA4IoT.Actuators.BinaryStateActuators
{
    public class MonostableLamp : StateMachine, IMonostableLamp
    {
        private readonly ISchedulerService _schedulerService;
        private readonly int ON_TIME = 300;
        private bool _ControledStateChange = false;

        public MonostableLamp(ComponentId id, IBinaryStateEndpoint endpoint, IBinaryInput input, ISchedulerService schedulerService)
            : base(id)
        {
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));
            if (schedulerService == null) throw new ArgumentNullException(nameof(schedulerService));
            
            _schedulerService = schedulerService;
            input.StateChanged += Input_StateChanged;

            AddState(new StateMachineState(BinaryStateId.Off).WithAction((par) =>
            {
                var inputState = input.Read();
                Debug.WriteLine($"[LIGHT] Set Lamp {Id} to OFF. Input state: {inputState}");

                if (inputState == BinaryState.Low)
                {
                    return;
                }

                _ControledStateChange = true; 
                   
                endpoint.TurnOn();

                _schedulerService.In(TimeSpan.FromMilliseconds(ON_TIME)).Execute(() =>
                {
                    endpoint.TurnOff();
                    _ControledStateChange = false;
                });
            }));

            AddState(new StateMachineState(BinaryStateId.On).WithAction((par) =>
            {
                var inputState = input.Read();
                Debug.WriteLine($"[LIGHT] Set Lamp {Id} to ON. Input state: {inputState}");

                if (inputState == BinaryState.High)
                {
                    return;
                }

                _ControledStateChange = true;
                endpoint.TurnOn();

                _schedulerService.In(TimeSpan.FromMilliseconds(ON_TIME)).Execute(() =>
                {
                    endpoint.TurnOff();
                    _ControledStateChange = false;
                });
            }));
        }

        private void Input_StateChanged(Object sender, BinaryStateChangedEventArgs e)
        {
            if(!_ControledStateChange)
            {
                if(e.NewState == BinaryState.High)
                {
                    SetState(BinaryStateId.On);
                }
                else
                {
                    SetState(BinaryStateId.Off);
                }
            }
        }

    }
}