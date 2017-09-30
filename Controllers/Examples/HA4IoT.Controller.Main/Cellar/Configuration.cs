using System;
using System.Threading.Tasks;
using Wirehome.Actuators;
using Wirehome.Actuators.Lamps;
using Wirehome.Actuators.StateMachines;
using Wirehome.Areas;
using Wirehome.Automations;
using Wirehome.Components;
using Wirehome.Contracts.Areas;
using Wirehome.Contracts.Components.Commands;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Hardware;
using Wirehome.Contracts.Hardware.RaspberryPi;
using Wirehome.Contracts.Messaging;
using Wirehome.Hardware;
using Wirehome.Hardware.Drivers.CCTools.Devices;
using Wirehome.Sensors;
using Wirehome.Sensors.Buttons;

namespace Wirehome.Controller.Main.Cellar
{
    internal sealed class Configuration : IConfiguration
    {
        private readonly IDeviceRegistryService _deviceRegistryService;
        private readonly IGpioService _gpioService;
        private readonly IAreaRegistryService _areaService;
        private readonly ActuatorFactory _actuatorFactory;
        private readonly SensorFactory _sensorFactory;
        private readonly AutomationFactory _automationFactory;
        private readonly IMessageBrokerService _messageBroker;

        public Configuration(
            IDeviceRegistryService deviceRegistryService,
            IGpioService gpioService,
            IAreaRegistryService areaService,
            ActuatorFactory actuatorFactory,
            SensorFactory sensorFactory,
            AutomationFactory automationFactory,
            IMessageBrokerService messageBroker)
        {
            _deviceRegistryService = deviceRegistryService ?? throw new ArgumentNullException(nameof(deviceRegistryService));
            _gpioService = gpioService ?? throw new ArgumentNullException(nameof(gpioService));
            _areaService = areaService ?? throw new ArgumentNullException(nameof(areaService));
            _messageBroker = messageBroker ?? throw new ArgumentNullException(nameof(messageBroker));
            _actuatorFactory = actuatorFactory ?? throw new ArgumentNullException(nameof(actuatorFactory));
            _sensorFactory = sensorFactory ?? throw new ArgumentNullException(nameof(sensorFactory));
            _automationFactory = automationFactory ?? throw new ArgumentNullException(nameof(automationFactory));
        }

        public Task ApplyAsync()
        {
            var hsrt16 = _deviceRegistryService.GetDevice<HSRT16>("HSRT16");

            var garden = _areaService.RegisterArea("Garden");

            var parkingLotOutput = new LogicalBinaryOutput(hsrt16[HSRT16Pin.Relay6], hsrt16[HSRT16Pin.Relay7], hsrt16[HSRT16Pin.Relay8]);
            _actuatorFactory.RegisterLamp(garden, Garden.LampParkingLot, parkingLotOutput);
            // Relay 9 is free.
            _actuatorFactory.RegisterSocket(garden, Garden.SocketPavillion, hsrt16[HSRT16Pin.Relay10]);
            _actuatorFactory.RegisterLamp(garden, Garden.LampRearArea, hsrt16[HSRT16Pin.Relay11]);
            _actuatorFactory.RegisterLamp(garden, Garden.SpotlightRoof, hsrt16[HSRT16Pin.Relay12]);
            _actuatorFactory.RegisterLamp(garden, Garden.LampTap, hsrt16[HSRT16Pin.Relay13]);
            _actuatorFactory.RegisterLamp(garden, Garden.LampGarage, hsrt16[HSRT16Pin.Relay14]);
            _actuatorFactory.RegisterLamp(garden, Garden.LampTerrace, hsrt16[HSRT16Pin.Relay15]);
            var stateMachine = _actuatorFactory.RegisterStateMachine(garden, Garden.StateMachine, InitializeStateMachine);
            
            var button = _sensorFactory.RegisterButton(garden, Garden.Button, _gpioService.GetInput(4, GpioPullMode.High, GpioInputMonitoringMode.Interrupt).WithInvertedState());

            button.CreatePressedShortTrigger(_messageBroker).Attach(() => stateMachine.TrySetNextState());
            button.CreatePressedLongTrigger(_messageBroker).Attach(() => stateMachine.TryTurnOff());

            _automationFactory.RegisterConditionalOnAutomation(garden, Garden.LampParkingLotAutomation)
                .WithComponent(garden.GetLamp(Garden.LampParkingLot))
                .WithOnAtNightRange()
                .WithOffBetweenRange(TimeSpan.Parse("22:30:00"), TimeSpan.Parse("05:00:00"));

            return Task.FromResult(0);
        }

        private void InitializeStateMachine(StateMachine stateMachine, IArea garden)
        {
            var turnOffCommand = new TurnOffCommand();
            var turnOnCommand = new TurnOnCommand();

            stateMachine.ResetStateId = StateMachineStateExtensions.OffStateId;

            stateMachine.AddOffState()
                .WithCommand(garden.GetComponent(Garden.LampTerrace), turnOffCommand)
                .WithCommand(garden.GetLamp(Garden.LampTerrace), turnOffCommand)
                .WithCommand(garden.GetLamp(Garden.LampGarage), turnOffCommand)
                .WithCommand(garden.GetLamp(Garden.LampTap), turnOffCommand)
                .WithCommand(garden.GetLamp(Garden.SpotlightRoof), turnOffCommand)
                .WithCommand(garden.GetLamp(Garden.LampRearArea), turnOffCommand);

            stateMachine.AddState("Te")
                .WithCommand(garden.GetLamp(Garden.LampTerrace), turnOnCommand)
                .WithCommand(garden.GetLamp(Garden.LampGarage), turnOffCommand)
                .WithCommand(garden.GetLamp(Garden.LampTap), turnOffCommand)
                .WithCommand(garden.GetLamp(Garden.SpotlightRoof), turnOffCommand)
                .WithCommand(garden.GetLamp(Garden.LampRearArea), turnOffCommand);

            stateMachine.AddState("G")
                .WithCommand(garden.GetLamp(Garden.LampTerrace), turnOffCommand)
                .WithCommand(garden.GetLamp(Garden.LampGarage), turnOnCommand)
                .WithCommand(garden.GetLamp(Garden.LampTap), turnOffCommand)
                .WithCommand(garden.GetLamp(Garden.SpotlightRoof), turnOffCommand)
                .WithCommand(garden.GetLamp(Garden.LampRearArea), turnOffCommand);

            stateMachine.AddState("W")
                .WithCommand(garden.GetLamp(Garden.LampTerrace), turnOffCommand)
                .WithCommand(garden.GetLamp(Garden.LampGarage), turnOffCommand)
                .WithCommand(garden.GetLamp(Garden.LampTap), turnOnCommand)
                .WithCommand(garden.GetLamp(Garden.SpotlightRoof), turnOffCommand)
                .WithCommand(garden.GetLamp(Garden.LampRearArea), turnOffCommand);

            stateMachine.AddState("D")
                .WithCommand(garden.GetLamp(Garden.LampTerrace), turnOffCommand)
                .WithCommand(garden.GetLamp(Garden.LampGarage), turnOffCommand)
                .WithCommand(garden.GetLamp(Garden.LampTap), turnOffCommand)
                .WithCommand(garden.GetLamp(Garden.SpotlightRoof), turnOnCommand)
                .WithCommand(garden.GetLamp(Garden.LampRearArea), turnOffCommand);

            stateMachine.AddState("Ti")
                .WithCommand(garden.GetLamp(Garden.LampTerrace), turnOffCommand)
                .WithCommand(garden.GetLamp(Garden.LampGarage), turnOffCommand)
                .WithCommand(garden.GetLamp(Garden.LampTap), turnOffCommand)
                .WithCommand(garden.GetLamp(Garden.SpotlightRoof), turnOffCommand)
                .WithCommand(garden.GetLamp(Garden.LampRearArea), turnOnCommand);

            stateMachine.AddState("G+W")
                .WithCommand(garden.GetLamp(Garden.LampTerrace), turnOffCommand)
                .WithCommand(garden.GetLamp(Garden.LampGarage), turnOnCommand)
                .WithCommand(garden.GetLamp(Garden.LampTap), turnOnCommand)
                .WithCommand(garden.GetLamp(Garden.SpotlightRoof), turnOffCommand)
                .WithCommand(garden.GetLamp(Garden.LampRearArea), turnOffCommand);

            stateMachine.AddState("Te+G+W")
                .WithCommand(garden.GetLamp(Garden.LampTerrace), turnOnCommand)
                .WithCommand(garden.GetLamp(Garden.LampGarage), turnOnCommand)
                .WithCommand(garden.GetLamp(Garden.LampTap), turnOnCommand)
                .WithCommand(garden.GetLamp(Garden.SpotlightRoof), turnOffCommand)
                .WithCommand(garden.GetLamp(Garden.LampRearArea), turnOffCommand);

            stateMachine.AddOnState()
                .WithCommand(garden.GetLamp(Garden.LampTerrace), turnOnCommand)
                .WithCommand(garden.GetLamp(Garden.LampGarage), turnOnCommand)
                .WithCommand(garden.GetLamp(Garden.LampTap), turnOnCommand)
                .WithCommand(garden.GetLamp(Garden.SpotlightRoof), turnOnCommand)
                .WithCommand(garden.GetLamp(Garden.LampRearArea), turnOnCommand);
        }
    }
}
