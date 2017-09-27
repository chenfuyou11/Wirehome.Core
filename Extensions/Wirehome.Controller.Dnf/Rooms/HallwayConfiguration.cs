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
using Wirehome.Extensions.Contracts;
using Wirehome.Extensions.Core;
using Wirehome.Sensors.MotionDetectors;
using Wirehome.Actuators.Lamps;
using Wirehome.Contracts.Components;

namespace Wirehome.Controller.Dnf.Rooms
{
    internal partial class HallwayConfiguration 
    {
        private readonly IDeviceRegistryService _deviceService;
        private readonly IAreaRegistryService _areaService;
        private readonly SensorFactory _sensorFactory;
        private readonly ActuatorFactory _actuatorFactory;
        private readonly AutomationFactory _automationFactory;
        private readonly IAlexaDispatcherEndpointService _alexaService;

        public HallwayConfiguration(IDeviceRegistryService deviceService,
                                    IAreaRegistryService areaService,
                                    SensorFactory sensorFactory,
                                    ActuatorFactory actuatorFactory,
                                    AutomationFactory automationFactory,
                                    IAlexaDispatcherEndpointService alexaService
                                    )  
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
            var input = _deviceService.GetDevice<HSPE16InputOnly>(CCToolsDevices.HSPE16_16.ToString());
            var input2 = _deviceService.GetDevice<HSPE16InputOnly>(CCToolsDevices.HSPE16_88.ToString());

            var relays = _deviceService.GetDevice<HSREL8>(CCToolsDevices.HSRel8_32.ToString());            
            var room = _areaService.RegisterArea(Room.Hallway);
            
            var m1 = _sensorFactory.RegisterMotionDetector(room, HallwayElements.MotionDetectorToilet, input[HSPE16Pin.GPIO5]);
            var m2 = _sensorFactory.RegisterMotionDetector(room, HallwayElements.MotionDetectorLivingroom, input[HSPE16Pin.GPIO6]);

            var lamp1 = _actuatorFactory.RegisterMonostableLamp(room, HallwayElements.Light_Two, new MonostableBinaryOutputAdapter(relays[HSREL8Pin.Relay3], input2[HSPE16Pin.GPIO14]));
            var lamp2 = _actuatorFactory.RegisterMonostableLamp(room, HallwayElements.Light_One, new MonostableBinaryOutputAdapter(relays[HSREL8Pin.Relay4], input2[HSPE16Pin.GPIO13]));

            var toiletAutomation = _automationFactory.RegisterTurnOnAndOffAutomation(room, HallwayElements.LightToiletAutomation)
             .WithTrigger(room.GetMotionDetector(HallwayElements.MotionDetectorToilet))
             .WithTarget(room.GetLamp(HallwayElements.Light_Two))
             .WithEnabledAtNight(TimeSpan.FromMinutes(-30), TimeSpan.FromMinutes(0));


            var livingRoomAutomation = _automationFactory.RegisterTurnOnAndOffAutomation(room, HallwayElements.LightLivingroomAutomation)
             .WithTrigger(room.GetMotionDetector(HallwayElements.MotionDetectorLivingroom))
             .WithTarget(room.GetLamp(HallwayElements.Light_One))
             .WithEnabledAtNight(TimeSpan.FromMinutes(-30), TimeSpan.FromMinutes(0));

            _alexaService.AddConnectedVivices("Hallway Light", new IComponent[] { lamp1, lamp2 });

        }


    }
}
