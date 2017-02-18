using System;
using HA4IoT.Actuators;
using HA4IoT.Automations;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.I2CHardwareBridge;
using HA4IoT.Sensors;
using HA4IoT.Sensors.MotionDetectors;
using HA4IoT.Services.Areas;
using HA4IoT.Services.Devices;
using System.Diagnostics;
using HA4IoT.Controller.Dnf.Enums;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Extensions.Extensions;
using HA4IoT.Extensions;
using HA4IoT.Contracts.Components;

namespace HA4IoT.Controller.Dnf.Rooms
{
    internal partial class LivingroomConfiguration
    {
        private readonly IDeviceService _deviceService;
        private readonly IAreaService _areaService;
        private readonly SensorFactory _sensorFactory;
        private readonly ActuatorFactory _actuatorFactory;
        private readonly AutomationFactory _automationFactory;
        private readonly IAlexaDispatcherEndpointService _alexaService;

        public LivingroomConfiguration(IDeviceService deviceService,
                                    IAreaService areaService,
                                    SensorFactory sensorFactory,
                                    ActuatorFactory actuatorFactory,
                                    AutomationFactory automationFactory,
                                    IAlexaDispatcherEndpointService alexaService)
        {
            _deviceService = deviceService;
            _areaService = areaService;
            _sensorFactory = sensorFactory;
            _actuatorFactory = actuatorFactory;
            _automationFactory = automationFactory;
            _alexaService = alexaService;
        }

        public void Apply()
        {
            var input = _deviceService.GetDevice<HSPE16InputOnly>(CCToolsDevices.HSPE16_16);
            var relays = _deviceService.GetDevice<HSREL8>(CCToolsDevices.HSRel8_32);
            var tempSensor = _deviceService.GetTempSensor((int)LivingroomElements.TempSensor);
            var humiditySensor = _deviceService.GetHumiditySensor((int)LivingroomElements.TempSensor);

            var room = _areaService.CreateArea(Room.LivingRoom);

            _sensorFactory.RegisterTemperatureSensor(room, LivingroomElements.TempSensor, tempSensor);
            _sensorFactory.RegisterHumiditySensor(room, LivingroomElements.HumiditySensor, humiditySensor);
            _sensorFactory.RegisterMotionDetector(room, LivingroomElements.MotionDetector, input[HSPE16Pin.GPIO0]);

            var lamp1 = _actuatorFactory.RegisterMonostableLamp(room, LivingroomElements.TVLight, relays[HSREL8Pin.Relay0], input[HSPE16Pin.GPIO14]);
            var lamp2 = _actuatorFactory.RegisterMonostableLamp(room, LivingroomElements.BedLight, relays[HSREL8Pin.Relay1], input[HSPE16Pin.GPIO13]);

            _alexaService.AddConnectedVivices("Light", new IComponent[] { lamp1, lamp2 });

            //_automationFactory.RegisterTurnOnAndOffAutomation(room, LivingroomElements.LightAutomation)
            // .WithTrigger(room.GetMotionDetector(LivingroomElements.MotionDetector))
            // .WithTarget(room.GetMonostableLamp(LivingroomElements.MainLight));


        }


    }
}
