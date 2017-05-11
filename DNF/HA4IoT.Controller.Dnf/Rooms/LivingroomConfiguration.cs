using HA4IoT.Actuators;
using HA4IoT.Automations;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Sensors;
using HA4IoT.Controller.Dnf.Enums;
using HA4IoT.Extensions.Extensions;
using HA4IoT.Extensions;
using HA4IoT.Contracts.Components;
using System;
using HA4IoT.Hardware.CCTools.Devices;
using HA4IoT.Services.Areas;
using HA4IoT.Extensions.Core;
using Windows.Media.Playback;
using Windows.Media.Core;
using Windows.Storage;

namespace HA4IoT.Controller.Dnf.Rooms
{
    internal partial class LivingroomConfiguration
    {
        private readonly IDeviceRegistryService _deviceService;
        private readonly IAreaRegistryService _areaService;
        private readonly SensorFactory _sensorFactory;
        private readonly ActuatorFactory _actuatorFactory;
        private readonly AutomationFactory _automationFactory;
        private readonly IAlexaDispatcherEndpointService _alexaService;
        private readonly ISchedulerService _schedulerService;

        public LivingroomConfiguration(IDeviceRegistryService deviceService,
                                    IAreaRegistryService areaService,
                                    SensorFactory sensorFactory,
                                    ActuatorFactory actuatorFactory,
                                    AutomationFactory automationFactory,
                                    IAlexaDispatcherEndpointService alexaService,
                                    ISchedulerService schedulerService)
        {
            _deviceService = deviceService;
            _areaService = areaService;
            _sensorFactory = sensorFactory;
            _actuatorFactory = actuatorFactory;
            _automationFactory = automationFactory;
            _alexaService = alexaService;
            _schedulerService = schedulerService;
        }

        public void Apply()
        {
            var input = _deviceService.GetDevice<HSPE16InputOnly>(CCToolsDevices.HSPE16_16.ToString());
            var relays = _deviceService.GetDevice<HSREL8>(CCToolsDevices.HSRel8_32.ToString());
            var tempSensor = _deviceService.GetTempSensor((int)LivingroomElements.TempSensor);
            var humiditySensor = _deviceService.GetHumiditySensor((int)LivingroomElements.TempSensor);

            var room = _areaService.RegisterArea(Room.LivingRoom);

            _sensorFactory.RegisterTemperatureSensor(room, LivingroomElements.TempSensor, tempSensor);
            _sensorFactory.RegisterHumiditySensor(room, LivingroomElements.HumiditySensor, humiditySensor);
            var md = _sensorFactory.RegisterMotionDetector(room, LivingroomElements.MotionDetector, input[HSPE16Pin.GPIO0]);

            var lamp1 = _actuatorFactory.RegisterMonostableLamp(room, LivingroomElements.TVLight, new MonostableBinaryOutputAdapter(relays[HSREL8Pin.Relay0], input[HSPE16Pin.GPIO14], _schedulerService));
            var lamp2 = _actuatorFactory.RegisterMonostableLamp(room, LivingroomElements.BedLight, new MonostableBinaryOutputAdapter(relays[HSREL8Pin.Relay1], input[HSPE16Pin.GPIO13], _schedulerService));

            _alexaService.AddConnectedVivices("Light", new IComponent[] { lamp1, lamp2 });

            //_automationFactory.RegisterTurnOnAndOffAutomation(room, LivingroomElements.LightAutomation)
            // .WithTrigger(room.GetMotionDetector(LivingroomElements.MotionDetector))
            // .WithTarget(room.GetMonostableLamp(LivingroomElements.MainLight));

            //md.MotionDetectedTrigger.Triggered += MotionDetectedTrigger_Triggered;

            //var speaker = new Speaker("speaker", new System.Collections.Generic.Dictionary<Enum, string>
            //    {
            //        { BirdSounds.Falcon, "Assets/falcon.mp3" }
            //    }
            //);

            //speaker.ExecuteCommand(new PlayCommand(BirdSounds.Falcon));

            //var livingRoomAutomation = _automationFactory.RegisterTurnOnAndOffAutomation(room, LivingroomElements.SchedulerAutomation)
            //.WithSchedulerTime(new SchedulerConfiguration
            //{
             //   StartTime = new TimeSpan(18, 0, 0),
             //   TurnOnTimeSpan = new TimeSpan(0, 1, 0)
           // });


        }

    }

    public enum BirdSounds
    {
        Falcon
    }
}
