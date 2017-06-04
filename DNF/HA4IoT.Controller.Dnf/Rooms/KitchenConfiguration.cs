using HA4IoT.Actuators;
using HA4IoT.Automations;
using HA4IoT.Sensors;
using HA4IoT.Controller.Dnf.Enums;
using HA4IoT.Extensions.Extensions;
using HA4IoT.Contracts.Areas;
using HA4IoT.Hardware.CCTools.Devices;
using HA4IoT.Extensions.Core;
using HA4IoT.Contracts.Core;
using HA4IoT.Areas;
using HA4IoT.Sensors.MotionDetectors;
using HA4IoT.Actuators.Lamps;
using System;

namespace HA4IoT.Controller.Dnf.Rooms
{
    internal partial class KitchenConfiguration 
    {
        private readonly IDeviceRegistryService _deviceService;
        private readonly IAreaRegistryService _areaService;
        private readonly SensorFactory _sensorFactory;
        private readonly ActuatorFactory _actuatorFactory;
        private readonly AutomationFactory _automationFactory;
        private readonly ISchedulerService _schedulerService;

        public KitchenConfiguration(IDeviceRegistryService deviceService,
                                    IAreaRegistryService areaService,
                                    SensorFactory sensorFactory,
                                    ActuatorFactory actuatorFactory,
                                    AutomationFactory automationFactory,
                                    ISchedulerService schedulerService)  
        {
            _deviceService = deviceService;
            _areaService = areaService;
            _sensorFactory = sensorFactory;
            _actuatorFactory = actuatorFactory;
            _automationFactory = automationFactory;
            _schedulerService = schedulerService;
        }

        public  void Apply()
        {
            var input = _deviceService.GetDevice<HSPE16InputOnly>(CCToolsDevices.HSPE16_16.ToString());
            var input2 = _deviceService.GetDevice<HSPE16InputOnly>(CCToolsDevices.HSPE16_88.ToString());

            var relays = _deviceService.GetDevice<HSREL8>(CCToolsDevices.HSRel8_32.ToString());
            var tempSensor = _deviceService.GetTempSensor((int)KitchenElements.TempSensor);
            var humiditySensor = _deviceService.GetHumiditySensor((int)KitchenElements.TempSensor);

            var room = _areaService.RegisterArea(Room.Kitchen);

            _sensorFactory.RegisterTemperatureSensor(room, KitchenElements.TempSensor, tempSensor);
            _sensorFactory.RegisterHumiditySensor(room, KitchenElements.HumiditySensor, humiditySensor);
            _sensorFactory.RegisterMotionDetector(room, KitchenElements.MotionDetector, input[HSPE16Pin.GPIO4]);

            _actuatorFactory.RegisterMonostableLamp(room, KitchenElements.Light, new MonostableBinaryOutputAdapter(relays[HSREL8Pin.Relay5], input2[HSPE16Pin.GPIO12], _schedulerService));

            _automationFactory.RegisterTurnOnAndOffAutomation(room, KitchenElements.LightAutomation)
             .WithTrigger(room.GetMotionDetector(KitchenElements.MotionDetector))
             .WithTarget(room.GetLamp(KitchenElements.Light))
             .WithEnabledAtNight(TimeSpan.FromMinutes(-30), TimeSpan.FromMinutes(0));


        }


       
    }
}
