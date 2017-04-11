using HA4IoT.Actuators;
using HA4IoT.Automations;
using HA4IoT.Sensors;
using HA4IoT.Controller.Dnf.Enums;
using HA4IoT.Extensions.Extensions;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Contracts.Areas;
using HA4IoT.Hardware.CCTools;
using System;
using HA4IoT.Hardware.CCTools.Devices;
using HA4IoT.Services.Areas;

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

        private const int TIME_TO_ON = 30;
        private const int TIME_WHILE_ON = 5;

      
        public BalconyConfiguration(IDeviceRegistryService deviceService, 
                                    IAreaRegistryService areaService,
                                    CCToolsDeviceService ccToolsBoardService,
                                    SensorFactory sensorFactory,
                                    ActuatorFactory actuatorFactory,
                                    AutomationFactory automationFactory) 
        {
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
            _areaService = areaService ?? throw new ArgumentNullException(nameof(areaService));
            _ccToolsBoardService = ccToolsBoardService ?? throw new ArgumentNullException(nameof(ccToolsBoardService));
            _actuatorFactory = actuatorFactory ?? throw new ArgumentNullException(nameof(actuatorFactory));
            _sensorFactory = sensorFactory ?? throw new ArgumentNullException(nameof(sensorFactory));
            _automationFactory = automationFactory ?? throw new ArgumentNullException(nameof(automationFactory));
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

            _sensorFactory.RegisterMotionDetector(room, BalconyElements.MotionDetector, input[HSPE16Pin.GPIO7]);

            _actuatorFactory.RegisterLamp(room, BalconyElements.Light, relays[HSREL8Pin.Relay0]);
          
            //var brennenstuhl = new BrennenstuhlCodeSequenceProvider();
            //var remoteSocket = RemoteSocketController.WithRemoteSocket((int)RemoteSockets.RemoteSocket_One, brennenstuhl.GetSequencePair(BrennenstuhlSystemCode.AllOn, BrennenstuhlUnitCode.D));

            //_actuatorFactory.RegisterSocket(room, (BalconyElements.RemoteSocket, remoteSocket.GetOutput((int)RemoteSockets.RemoteSocket_One));

            //room.SetupTurnOnAndOffAutomation()
            //    .WithTrigger(new IntervalTrigger(TimeSpan.FromSeconds(TIME_TO_ON), this.Controller.ServiceLocator.GetService<ISchedulerService>()))
            //    .WithTarget(room.Socket(BalconyElements.RemoteSocket))
            //    .WithOnDuration(TimeSpan.FromSeconds(TIME_WHILE_ON));
        }

    }
}
