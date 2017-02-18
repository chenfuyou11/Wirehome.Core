using HA4IoT.Actuators;
using HA4IoT.Automations;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.I2CHardwareBridge;
using HA4IoT.Sensors;
using HA4IoT.Services.Areas;
using HA4IoT.Services.Devices;
using HA4IoT.Controller.Dnf.Enums;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Sensors.MotionDetectors;
using HA4IoT.Extensions;
using HA4IoT.Contracts.Components;
using System;
using System.Diagnostics;

namespace HA4IoT.Controller.Dnf.Rooms
{
    internal partial class HallwayConfiguration 
    {
        private readonly IDeviceService _deviceService;
        private readonly IAreaService _areaService;
        private readonly SensorFactory _sensorFactory;
        private readonly ActuatorFactory _actuatorFactory;
        private readonly AutomationFactory _automationFactory;
        private readonly IAlexaDispatcherEndpointService _alexaService;

        public HallwayConfiguration(IDeviceService deviceService,
                                    IAreaService areaService,
                                    SensorFactory sensorFactory,
                                    ActuatorFactory actuatorFactory,
                                    AutomationFactory automationFactory,
                                    IAlexaDispatcherEndpointService alexaService)  
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
            var input = _deviceService.GetDevice<HSPE16InputOnly>(CCToolsDevices.HSPE16_16);
            var relays = _deviceService.GetDevice<HSREL8>(CCToolsDevices.HSRel8_32);            
            var room = _areaService.CreateArea(Room.Hallway);
            
            var m1 = _sensorFactory.RegisterMotionDetector(room, HallwayElements.MotionDetectorToilet, input[HSPE16Pin.GPIO5]);
            var m2 = _sensorFactory.RegisterMotionDetector(room, HallwayElements.MotionDetectorLivingroom, input[HSPE16Pin.GPIO6]);

            var lamp1 = _actuatorFactory.RegisterMonostableLamp(room, HallwayElements.Light_Two, relays[HSREL8Pin.Relay3], input[HSPE16Pin.GPIO12]);
            var lamp2 = _actuatorFactory.RegisterMonostableLamp(room, HallwayElements.Light_One, relays[HSREL8Pin.Relay4], input[HSPE16Pin.GPIO11]);

            var toiletAutomation = _automationFactory.RegisterTurnOnAndOffAutomation(room, HallwayElements.LightToiletAutomation)
             .WithTrigger(room.GetMotionDetector(HallwayElements.MotionDetectorToilet))
             .WithTarget(room.GetMonostableLamp(HallwayElements.Light_Two));
            

            var livingRoomAutomation = _automationFactory.RegisterTurnOnAndOffAutomation(room, HallwayElements.LightLivingroomAutomation)
             .WithTrigger(room.GetMotionDetector(HallwayElements.MotionDetectorLivingroom))
             .WithTarget(room.GetMonostableLamp(HallwayElements.Light_One));

            _alexaService.AddConnectedVivices("Hallway Light", new IComponent[] { lamp1, lamp2 });


            m1.GetMotionDetectedTrigger().Triggered += MotionDetected;
            m2.GetMotionDetectedTrigger().Triggered += MotionDetected2; 

        }

        private void MotionDetected2(object sender, Contracts.Triggers.TriggeredEventArgs e)
        {
            Debug.WriteLine($"[Motion] detected_1 [{DateTime.Now:hh:mm:ss}]");
        }

        private void MotionDetected(object sender, Contracts.Triggers.TriggeredEventArgs e)
        {
            Debug.WriteLine($"[Motion] detected_2 [{DateTime.Now:hh:mm:ss}]");
        }

    }
}
