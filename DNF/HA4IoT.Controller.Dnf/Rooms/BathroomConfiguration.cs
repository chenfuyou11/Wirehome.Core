using HA4IoT.Actuators;
using HA4IoT.Automations;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.I2CHardwareBridge;
using HA4IoT.Sensors;
using HA4IoT.Services.Areas;
using HA4IoT.Services.Devices;
using System.Diagnostics;
using HA4IoT.Controller.Dnf.Enums;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Sensors.MotionDetectors;
using HA4IoT.Extensions.Extensions;

namespace HA4IoT.Controller.Dnf.Rooms
{
    internal partial class BathroomConfiguration 
    {
        private readonly IDeviceService _deviceService;
        private readonly IAreaService _areaService;
        private readonly SensorFactory _sensorFactory;
        private readonly ActuatorFactory _actuatorFactory;
        private readonly AutomationFactory _automationFactory;

        public BathroomConfiguration(IDeviceService deviceService,
                                    IAreaService areaService,
                                    SensorFactory sensorFactory,
                                    ActuatorFactory actuatorFactory,
                                    AutomationFactory automationFactory) 
        {
            _deviceService = deviceService;
            _areaService = areaService;
            _sensorFactory = sensorFactory;
            _actuatorFactory = actuatorFactory;
            _automationFactory = automationFactory;
        }

        public void Apply()
        {
            var input = _deviceService.GetDevice<HSPE16InputOnly>(CCToolsDevices.HSPE16_16);
            var input_88 = _deviceService.GetDevice<HSPE16InputOnly>(CCToolsDevices.HSPE16_88);
            var relays = _deviceService.GetDevice<HSREL8>(CCToolsDevices.HSRel8_24);
            var tempSensor = _deviceService.GetTempSensor((int)BathroomElements.TempSensor);
            var humiditySensor = _deviceService.GetHumiditySensor((int)BathroomElements.TempSensor);

            var room = _areaService.CreateArea(Room.Bathroom);

            _sensorFactory.RegisterTemperatureSensor(room, BathroomElements.TempSensor, tempSensor);
            _sensorFactory.RegisterHumiditySensor(room, BathroomElements.HumiditySensor, humiditySensor);
            _sensorFactory.RegisterMotionDetector(room, BathroomElements.MotionDetector, input[HSPE16Pin.GPIO2]);

            _actuatorFactory.RegisterMonostableLamp(room, BathroomElements.Light, relays[HSREL8Pin.Relay0], input[HSPE16Pin.GPIO9]);

            _automationFactory.RegisterTurnOnAndOffAutomation(room, BathroomElements.LightAutomation)
             .WithTrigger(room.GetMotionDetector(BathroomElements.MotionDetector))
             .WithTarget(room.GetMonostableLamp(BathroomElements.Light))
             .WithDisableTurnOffWhenBinaryStateEnabled(input_88[HSPE16Pin.GPIO1]);
            
        }

    }
}
