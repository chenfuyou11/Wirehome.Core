using Wirehome.Controller.Dnf.Enums;
using System;
using Wirehome.Actuators;
using Wirehome.Automations;
using Wirehome.Contracts.Areas;
using Wirehome.Contracts.Core;
using Wirehome.Hardware.Drivers.CCTools.Devices;
using Wirehome.Sensors;
using Wirehome.Extensions.Extensions;
using Wirehome.Areas;
using Wirehome.Extensions.Core;
using Wirehome.Sensors.MotionDetectors;
using Wirehome.Actuators.Lamps;

namespace Wirehome.Controller.Dnf.Rooms
{
    internal partial class KitchenConfiguration 
    {
        private readonly IDeviceRegistryService _deviceService;
        private readonly IAreaRegistryService _areaService;
        private readonly SensorFactory _sensorFactory;
        private readonly ActuatorFactory _actuatorFactory;
        private readonly AutomationFactory _automationFactory;

        public KitchenConfiguration(IDeviceRegistryService deviceService,
                                    IAreaRegistryService areaService,
                                    SensorFactory sensorFactory,
                                    ActuatorFactory actuatorFactory,
                                    AutomationFactory automationFactory
                                    )  
        {
            _deviceService = deviceService;
            _areaService = areaService;
            _sensorFactory = sensorFactory;
            _actuatorFactory = actuatorFactory;
            _automationFactory = automationFactory;
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

            _actuatorFactory.RegisterMonostableLamp(room, KitchenElements.Light, new MonostableBinaryOutputAdapter(relays[HSREL8Pin.Relay5], input2[HSPE16Pin.GPIO12]));

            _automationFactory.RegisterTurnOnAndOffAutomation(room, KitchenElements.LightAutomation)
             .WithTrigger(room.GetMotionDetector(KitchenElements.MotionDetector))
             .WithTarget(room.GetLamp(KitchenElements.Light))
             .WithEnabledAtNight(TimeSpan.FromMinutes(-30), TimeSpan.FromMinutes(0));


        }


       
    }
}
