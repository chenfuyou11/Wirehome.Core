using Wirehome.Controller.Dnf.Enums;
using Wirehome.Actuators;
using Wirehome.Automations;
using Wirehome.Contracts.Areas;
using Wirehome.Contracts.Core;
using Wirehome.Hardware.Drivers.CCTools.Devices;
using Wirehome.Sensors;
using Wirehome.Areas;
using Wirehome.Contracts.Hardware;

namespace Wirehome.Controller.Dnf.Rooms
{

    internal partial class StaircaseConfiguration
    {
        private readonly IDeviceRegistryService _deviceService;
        private readonly IAreaRegistryService _areaService;
        private readonly SensorFactory _sensorFactory;
        private readonly ActuatorFactory _actuatorFactory;
        private readonly AutomationFactory _automationFactory;

        public StaircaseConfiguration(IDeviceRegistryService deviceService,
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
            var input = _deviceService.GetDevice<HSPE16InputOnly>(CCToolsDevices.HSPE16_88.ToString());
            var relays = _deviceService.GetDevice<HSREL8>(CCToolsDevices.HSRel8_24.ToString());

            var room = _areaService.RegisterArea(Room.StairCase);

            var GPIO0 = input[HSPE16Pin.GPIO0].WithInvertedState();
           
            _sensorFactory.RegisterMotionDetector(room, StaircaseElements.MotionDetector, GPIO0);

            _actuatorFactory.RegisterLamp(room, ToiletElements.Light, relays[HSREL8Pin.Relay5]);

            //_automationFactory.RegisterTurnOnAndOffAutomation(room, StaircaseElements.LightAutomation)
            // .WithTrigger(room.GetMotionDetector(StaircaseElements.MotionDetector))
            // .WithTarget(room.GetLamp(StaircaseElements.Light));      
        }
    }
}
