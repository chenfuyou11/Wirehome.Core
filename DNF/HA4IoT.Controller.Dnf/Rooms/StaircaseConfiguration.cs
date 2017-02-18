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

    internal partial class StaircaseConfiguration
    {
        private readonly IDeviceService _deviceService;
        private readonly IAreaService _areaService;
        private readonly SensorFactory _sensorFactory;
        private readonly ActuatorFactory _actuatorFactory;
        private readonly AutomationFactory _automationFactory;

        public StaircaseConfiguration(IDeviceService deviceService,
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
            var input = _deviceService.GetDevice<HSPE16InputOnly>(CCToolsDevices.HSPE16_88);
            var relays = _deviceService.GetDevice<HSREL8>(CCToolsDevices.HSRel8_24);

            var room = _areaService.CreateArea(Room.StairCase);

            _sensorFactory.RegisterMotionDetector(room, StaircaseElements.MotionDetector, input[HSPE16Pin.GPIO0].WithInvertedState(true));

            _actuatorFactory.RegisterLamp(room, ToiletElements.Light, relays[HSREL8Pin.Relay5]);

            _automationFactory.RegisterTurnOnAndOffAutomation(room, StaircaseElements.LightAutomation)
             .WithTrigger(room.GetMotionDetector(StaircaseElements.MotionDetector))
             .WithTarget(room.GetLamp(StaircaseElements.Light));      
        }
    }
}
