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
using HA4IoT.Sensors.MotionDetectors;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Extensions.Extensions;

namespace HA4IoT.Controller.Dnf.Rooms
{
    internal partial class KitchenConfiguration 
    {
        private readonly IDeviceService _deviceService;
        private readonly IAreaService _areaService;
        private readonly SensorFactory _sensorFactory;
        private readonly ActuatorFactory _actuatorFactory;
        private readonly AutomationFactory _automationFactory;

        public KitchenConfiguration(IDeviceService deviceService,
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

        public  void Apply()
        {
            var input = _deviceService.GetDevice<HSPE16InputOnly>(CCToolsDevices.HSPE16_16);
            var relays = _deviceService.GetDevice<HSREL8>(CCToolsDevices.HSRel8_32);
            var tempSensor = _deviceService.GetTempSensor((int)KitchenElements.TempSensor);
            var humiditySensor = _deviceService.GetHumiditySensor((int)KitchenElements.TempSensor);

            var room = _areaService.CreateArea(Room.Kitchen);

            _sensorFactory.RegisterTemperatureSensor(room, KitchenElements.TempSensor, tempSensor);
            _sensorFactory.RegisterHumiditySensor(room, KitchenElements.HumiditySensor, humiditySensor);
            _sensorFactory.RegisterMotionDetector(room, KitchenElements.MotionDetector, input[HSPE16Pin.GPIO4]);

            _actuatorFactory.RegisterMonostableLamp(room, KitchenElements.Light, relays[HSREL8Pin.Relay5], input[HSPE16Pin.GPIO10]);

            _automationFactory.RegisterTurnOnAndOffAutomation(room, KitchenElements.LightAutomation)
             .WithTrigger(room.GetMotionDetector(KitchenElements.MotionDetector))
             .WithTarget(room.GetMonostableLamp(KitchenElements.Light));

        }


       
    }
}
