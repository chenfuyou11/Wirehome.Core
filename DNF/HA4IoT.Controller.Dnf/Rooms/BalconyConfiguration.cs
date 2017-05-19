using HA4IoT.Actuators;
using HA4IoT.Automations;
using HA4IoT.Sensors;
using HA4IoT.Controller.Dnf.Enums;
using HA4IoT.Extensions.Extensions;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Contracts.Areas;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.CCTools.Devices;
using HA4IoT.Services.Areas;
using System;
using HA4IoT.Extensions.Core;
using HA4IoT.Hardware.RemoteSwitch;
using HA4IoT.Contracts.Components.States;
using HA4IoT.Hardware.RemoteSwitch.Codes.Protocols;
using HA4IoT.Triggers;

namespace HA4IoT.Controller.Dnf.Rooms
{
    internal partial class BalconyConfiguration
    {
        private readonly IDeviceRegistryService _deviceService;
        private readonly IAreaRegistryService _areaService;
        private readonly SensorFactory _sensorFactory;
        private readonly ActuatorFactory _actuatorFactory;
        private readonly AutomationFactory _automationFactory;
        private readonly CCToolsDeviceService _ccToolsBoardService;
        private readonly RemoteSocketService _remoteSocketService;
        private readonly ISchedulerService _schedulerService;



        private const int TIME_TO_ON = 30;
        private const int TIME_WHILE_ON = 5;
        private Speaker _speaker;


        public BalconyConfiguration(IDeviceRegistryService deviceService, 
                                    IAreaRegistryService areaService,
                                    CCToolsDeviceService ccToolsBoardService,
                                    SensorFactory sensorFactory,
                                    ActuatorFactory actuatorFactory,
                                    AutomationFactory automationFactory,
                                    RemoteSocketService remoteSocketService,
                                    ISchedulerService schedulerService
                                    ) 
        {
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
            _areaService = areaService ?? throw new ArgumentNullException(nameof(areaService));
            _ccToolsBoardService = ccToolsBoardService ?? throw new ArgumentNullException(nameof(ccToolsBoardService));
            _actuatorFactory = actuatorFactory ?? throw new ArgumentNullException(nameof(actuatorFactory));
            _sensorFactory = sensorFactory ?? throw new ArgumentNullException(nameof(sensorFactory));
            _automationFactory = automationFactory ?? throw new ArgumentNullException(nameof(automationFactory));
            _remoteSocketService = remoteSocketService ?? throw new ArgumentNullException(nameof(remoteSocketService));
            _schedulerService = schedulerService;

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
            var relays = _deviceService.GetDevice<HSREL8>(CCToolsDevices.HSRel8_24.ToString());
            var tempSensor = _deviceService.GetTempSensor((int)BalconyElements.TempSensor);
            var humiditySensor = _deviceService.GetHumiditySensor((int)BalconyElements.TempSensor);

            var room = _areaService.RegisterArea(Room.Balcony);
            
            _sensorFactory.RegisterTemperatureSensor(room, BalconyElements.TempSensor, tempSensor);
            _sensorFactory.RegisterHumiditySensor(room, BalconyElements.HumiditySensor, humiditySensor);

            var md = _sensorFactory.RegisterMotionDetector(room, BalconyElements.MotionDetector, input[HSPE16Pin.GPIO7]);
            md.StateChanged += Md_StateChanged;

            _actuatorFactory.RegisterLamp(room, BalconyElements.Light, relays[HSREL8Pin.Relay0]);

            var codeSequenceProvider = new DipswitchCodeProvider();

            var codePair = codeSequenceProvider.GetCodePair(DipswitchSystemCode.AllOn, DipswitchUnitCode.D, 10);
    
            var socket = _actuatorFactory.RegisterSocket(room, BalconyElements.RemoteSocket, _remoteSocketService.RegisterRemoteSocket(BalconyElements.RemoteSocket.ToString(), codePair));
            
            //var livingRoomAutomation = _automationFactory.RegisterTurnOnAndOffAutomation(room, LivingroomElements.SchedulerAutomation)
            //.WithSchedulerTime(new SchedulerConfiguration
            //{
            //    StartTime = new TimeSpan(18, 0, 0),
            //    TurnOnInterval = new TimeSpan(0, 0, 5),
            //    WorkingTime = new TimeSpan(0, 0, 1)
            //})
            //.WithTarget(socket);
        }

        private void Md_StateChanged(object sender, Contracts.Components.ComponentFeatureStateChangedEventArgs e)
        {
            var state = e.NewState.Extract<MotionDetectionState>();
            if (state.Value == MotionDetectionStateValue.MotionDetected)
            {
                _speaker.PlayRandom();
            }
        }
    } 
}
