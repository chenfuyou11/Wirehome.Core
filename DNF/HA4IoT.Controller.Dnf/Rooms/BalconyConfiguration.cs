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
using HA4IoT.Extensions.Extensions;

namespace HA4IoT.Controller.Dnf.Rooms
{
    internal partial class BalconyConfiguration
    {
        private readonly IDeviceService _deviceService;
        private readonly IAreaService _areaService;
        private readonly SensorFactory _sensorFactory;
        private readonly ActuatorFactory _actuatorFactory;
        private readonly AutomationFactory _automationFactory;
   
        private const int TIME_TO_ON = 30;
        private const int TIME_WHILE_ON = 5;

      
        public BalconyConfiguration(IDeviceService deviceService, 
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
            var tempSensor = _deviceService.GetTempSensor((int)BalconyElements.TempSensor);
            var humiditySensor = _deviceService.GetHumiditySensor((int)BalconyElements.TempSensor);

            var room = _areaService.CreateArea(Room.Balcony);
            
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
