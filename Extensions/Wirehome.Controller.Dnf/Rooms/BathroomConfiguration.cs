using Wirehome.Controller.Dnf.Enums;
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
    internal partial class BathroomConfiguration 
    {
        private readonly IDeviceRegistryService _deviceService;
        private readonly IAreaRegistryService _areaService;
        private readonly SensorFactory _sensorFactory;
        private readonly ActuatorFactory _actuatorFactory;
        private readonly AutomationFactory _automationFactory;

        public BathroomConfiguration(IDeviceRegistryService deviceService,
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

        public void Apply()
        {
            var input = _deviceService.GetDevice<HSPE16InputOnly>(CCToolsDevices.HSPE16_16.ToString());
            var input_88 = _deviceService.GetDevice<HSPE16InputOnly>(CCToolsDevices.HSPE16_88.ToString());
            var relays = _deviceService.GetDevice<HSREL8>(CCToolsDevices.HSRel8_24.ToString());
            var tempSensor = _deviceService.GetTempSensor((int)BathroomElements.TempSensor);
            var humiditySensor = _deviceService.GetHumiditySensor((int)BathroomElements.TempSensor);

            var room = _areaService.RegisterArea(Room.Bathroom);

            _sensorFactory.RegisterTemperatureSensor(room, BathroomElements.TempSensor, tempSensor);
            _sensorFactory.RegisterHumiditySensor(room, BathroomElements.HumiditySensor, humiditySensor);
            _sensorFactory.RegisterMotionDetector(room, BathroomElements.MotionDetector, input[HSPE16Pin.GPIO2]);
            
            _actuatorFactory.RegisterMonostableLamp(room, BathroomElements.Light, new MonostableBinaryOutputAdapter(relays[HSREL8Pin.Relay0], input_88[HSPE16Pin.GPIO11]));
            
            _automationFactory.RegisterTurnOnAndOffAutomation(room, BathroomElements.LightAutomation)
           .WithTrigger(room.GetMotionDetector(BathroomElements.MotionDetector))
           .WithTarget(room.GetLamp(BathroomElements.Light))
            .WithDisableTurnOffWhenBinaryStateEnabled(input_88[HSPE16Pin.GPIO1]);
        }

    }
}
