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
using HA4IoT.Extensions.Extensions;

namespace HA4IoT.Controller.Dnf.Rooms
{
    internal partial class BedroomConfiguration 
    {
        private readonly IDeviceService _deviceService;
        private readonly IAreaService _areaService;
        private readonly SensorFactory _sensorFactory;
        private readonly ActuatorFactory _actuatorFactory;
        private readonly AutomationFactory _automationFactory;

        public BedroomConfiguration(IDeviceService deviceService,
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
            var relays = _deviceService.GetDevice<HSREL8>(CCToolsDevices.HSRel8_24);
            var tempSensor = _deviceService.GetTempSensor((int)BedroomElements.TempSensor);
            var humiditySensor = _deviceService.GetTempSensor((int)BedroomElements.TempSensor);
            
            //var currentController = _deviceService.GetDevice<CurrentController>();
            //var lightCurrentInputSensor = currentController.GetInput((int)BedroomElements.CurrentSensor);

            var room = _areaService.CreateArea(Room.Bedroom);

            _sensorFactory.RegisterTemperatureSensor(room, BedroomElements.TempSensor, tempSensor);
            _sensorFactory.RegisterHumiditySensor(room, BedroomElements.HumiditySensor, humiditySensor);

            _sensorFactory.RegisterMotionDetector(room, BedroomElements.MotionDetector, input[HSPE16Pin.GPIO1]);

            _actuatorFactory.RegisterLamp(room, BedroomElements.Light, relays[HSREL8Pin.Relay0]);
        }

    }
}
