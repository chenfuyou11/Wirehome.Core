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

namespace HA4IoT.Controller.Dnf.Rooms
{

    internal partial class ToiletConfiguration 
    {
        private readonly IDeviceRegistryService _deviceService;
        private readonly IAreaRegistryService _areaService;
        private readonly SensorFactory _sensorFactory;
        private readonly ActuatorFactory _actuatorFactory;
        private readonly AutomationFactory _automationFactory;
        private readonly ISchedulerService _schedulerService;

        public ToiletConfiguration(IDeviceRegistryService deviceService,
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

        public void Apply()
        {
            var input = _deviceService.GetDevice<HSPE16InputOnly>(CCToolsDevices.HSPE16_16.ToString());
            var input2 = _deviceService.GetDevice<HSPE16InputOnly>(CCToolsDevices.HSPE16_88.ToString());
            var relays = _deviceService.GetDevice<HSREL8>(CCToolsDevices.HSRel8_32.ToString());
            var tempSensor = _deviceService.GetTempSensor((int)ToiletElements.TempSensor);
            var humiditySensor = _deviceService.GetHumiditySensor((int)ToiletElements.TempSensor);

            var room = _areaService.RegisterArea(Room.Toilet);

            _sensorFactory.RegisterTemperatureSensor(room, ToiletElements.TempSensor, tempSensor);
            _sensorFactory.RegisterHumiditySensor(room, ToiletElements.HumiditySensor, humiditySensor);
            _sensorFactory.RegisterMotionDetector(room, ToiletElements.MotionDetector, input[HSPE16Pin.GPIO3]);

            _actuatorFactory.RegisterMonostableLamp(room, ToiletElements.Light, new MonostableBinaryOutputAdapter(relays[HSREL8Pin.Relay2], input2[HSPE16Pin.GPIO15], _schedulerService));

            _automationFactory.RegisterTurnOnAndOffAutomation(room, ToiletElements.LightAutomation)
             .WithTrigger(room.GetMotionDetector(ToiletElements.MotionDetector))
             .WithTarget(room.GetLamp(ToiletElements.Light));      
        }
    }
}
