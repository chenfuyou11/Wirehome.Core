using Wirehome.Controller.Dnf.Enums;
using Wirehome.Actuators;
using Wirehome.Automations;
using Wirehome.Contracts.Areas;
using Wirehome.Contracts.Core;
using Wirehome.Hardware.Drivers.CCTools.Devices;
using Wirehome.Sensors;
using Wirehome.Extensions.Extensions;
using Wirehome.Areas;

namespace Wirehome.Controller.Dnf.Rooms
{
    internal partial class BedroomConfiguration 
    {
        private readonly IDeviceRegistryService _deviceService;
        private readonly IAreaRegistryService _areaService;
        private readonly SensorFactory _sensorFactory;
        private readonly ActuatorFactory _actuatorFactory;
        private readonly AutomationFactory _automationFactory;

        public BedroomConfiguration(IDeviceRegistryService deviceService,
                                    IAreaRegistryService areaService,
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
            var input = _deviceService.GetDevice<HSPE16InputOnly>(CCToolsDevices.HSPE16_16.ToString());
            var relays = _deviceService.GetDevice<HSREL8>(CCToolsDevices.HSRel8_24.ToString());
            var tempSensor = _deviceService.GetTempSensor((int)BedroomElements.TempSensor);
            var humiditySensor = _deviceService.GetTempSensor((int)BedroomElements.TempSensor);
            
            //var currentController = _deviceService.GetDevice<CurrentController>();
            //var lightCurrentInputSensor = currentController.GetInput((int)BedroomElements.CurrentSensor);

            var room = _areaService.RegisterArea(Room.Bedroom);

            _sensorFactory.RegisterTemperatureSensor(room, BedroomElements.TempSensor, tempSensor);
            _sensorFactory.RegisterHumiditySensor(room, BedroomElements.HumiditySensor, humiditySensor);

            var md = _sensorFactory.RegisterMotionDetector(room, BedroomElements.MotionDetector, input[HSPE16Pin.GPIO1]);

            _actuatorFactory.RegisterLamp(room, BedroomElements.Light, relays[HSREL8Pin.Relay0]);
        }

        
    }
}
