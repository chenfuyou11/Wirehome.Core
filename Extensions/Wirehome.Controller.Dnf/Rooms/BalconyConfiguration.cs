using System;
using HA4IoT.Actuators;
using HA4IoT.Automations;
using HA4IoT.Sensors;
using HA4IoT.Controller.Dnf.Enums;
using HA4IoT.Extensions.Extensions;
using HA4IoT.Contracts.Areas;
using HA4IoT.Extensions.Core;
using HA4IoT.Contracts.Components.States;
using HA4IoT.Contracts.Core;
using HA4IoT.Hardware.RemoteSockets;
using HA4IoT.Areas;
using HA4IoT.Contracts.Hardware.RemoteSockets.Protocols;
using HA4IoT.Hardware.Drivers.CCTools;
using HA4IoT.Contracts.Scheduling;
using HA4IoT.Hardware.Drivers.CCTools.Devices;
using HA4IoT.Hardware.Drivers.RemoteSockets;
using HA4IoT.Contracts.Hardware.RemoteSockets;

namespace HA4IoT.Controller.Dnf.Rooms
{
    internal partial class BalconyConfiguration
    {
        private readonly IDeviceRegistryService _deviceService;
        private readonly IAreaRegistryService _areaService;
        private readonly SensorFactory _sensorFactory;
        private readonly ActuatorFactory _actuatorFactory;
        private readonly AutomationFactory _automationFactory;
        private readonly IRemoteSocketService _remoteSocketService;

        private const int TIME_TO_ON = 30;
        private const int TIME_WHILE_ON = 5;
        private Speaker _speaker;


        public BalconyConfiguration(IDeviceRegistryService deviceService, 
                                    IAreaRegistryService areaService,
                                    SensorFactory sensorFactory,
                                    ActuatorFactory actuatorFactory,
                                    AutomationFactory automationFactory,
                                    IRemoteSocketService remoteSocketService
                                    ) 
        {
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
            _areaService = areaService ?? throw new ArgumentNullException(nameof(areaService));
            _actuatorFactory = actuatorFactory ?? throw new ArgumentNullException(nameof(actuatorFactory));
            _sensorFactory = sensorFactory ?? throw new ArgumentNullException(nameof(sensorFactory));
            _automationFactory = automationFactory ?? throw new ArgumentNullException(nameof(automationFactory));
            _remoteSocketService = remoteSocketService ?? throw new ArgumentNullException(nameof(remoteSocketService));

            _speaker = new Speaker("speaker", new System.Collections.Generic.Dictionary<Enum, string>
                {
                    { BirdSounds.Eagle, "Assets/eagle.mp3" },
                    { BirdSounds.EagleBad, "Assets/eagle_bad.mp3" },
                    { BirdSounds.Falcon, "Assets/falcon.mp3" },
                    { BirdSounds.Falcon2, "Assets/falcon_2.mp3" },
                    { BirdSounds.Hawk, "Assets/hawk.mp3" },
                    { BirdSounds.Hawk2, "Assets/hawk_2.mp3" },
                    { BirdSounds.Owl, "Assets/owl.mp3" },
                    { BirdSounds.Owl2, "Assets/owl_2.mp3" },
                    { BirdSounds.Owl3, "Assets/owl_3.mp3" },
                    { BirdSounds.Owl4, "Assets/owl_4.mp3" }
                }
           );
        }

        public void Apply()
        {
            var input = _deviceService.GetDevice<HSPE16InputOnly>(CCToolsDevices.HSPE16_16.ToString());
            var input_2 = _deviceService.GetDevice<HSPE16InputOnly>(CCToolsDevices.HSPE16_88.ToString());
            var relays = _deviceService.GetDevice<HSREL8>(CCToolsDevices.HSRel8_24.ToString());
            var tempSensor = _deviceService.GetTempSensor((int)BalconyElements.TempSensor);
            var humiditySensor = _deviceService.GetHumiditySensor((int)BalconyElements.TempSensor);

            var room = _areaService.RegisterArea(Room.Balcony);
            
            _sensorFactory.RegisterTemperatureSensor(room, BalconyElements.TempSensor, tempSensor);
            _sensorFactory.RegisterHumiditySensor(room, BalconyElements.HumiditySensor, humiditySensor);

            var md =_sensorFactory.RegisterMotionDetector(room, BalconyElements.MotionDetector, input[HSPE16Pin.GPIO7]);
            var md2 = _sensorFactory.RegisterMotionDetector(room, BalconyElements.BirdMotionDetector, input_2[HSPE16Pin.GPIO2]);
            md2.StateChanged += Md_StateChanged;

            _actuatorFactory.RegisterLamp(room, BalconyElements.Light, relays[HSREL8Pin.Relay0]);

          
            var codePair = DipswitchCodeProvider.GetCodePair(DipswitchSystemCode.AllOn, DipswitchUnitCode.D, 20);
            var socket = _actuatorFactory.RegisterSocket(room, BalconyElements.RemoteSocket, _remoteSocketService.RegisterRemoteSocket(BalconyElements.RemoteSocket.ToString(), "RemoteSocketBridge", codePair));

            var livingRoomAutomation = _automationFactory.RegisterTurnOnAndOffAutomation(room, LivingroomElements.SchedulerAutomation)
            .WithSchedulerTime(new AutomationScheduler
            {
                StartTime = new TimeSpan(18, 0, 0),
                TurnOnInterval = new TimeSpan(0, 0, 5),
                WorkingTime = new TimeSpan(0, 0, 2)
            })
            .WithTarget(socket);
        }

        private void Md_StateChanged(object sender, Contracts.Components.ComponentFeatureStateChangedEventArgs e)
        {
            //var state = e.NewState.Extract<MotionDetectionState>();
            //if (state.Value == MotionDetectionStateValue.MotionDetected)
            //{
            //    _speaker.PlayRandom();
            //}
        }
    } 
}
