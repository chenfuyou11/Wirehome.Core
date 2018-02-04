using Wirehome.Controller.Dnf.Enums;
using System;
using Wirehome.Actuators;
using Wirehome.Automations;
using Wirehome.Contracts.Areas;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Hardware.RemoteSockets;
using Wirehome.Contracts.Hardware.RemoteSockets.Protocols;
using Wirehome.Hardware.Drivers.RemoteSockets;
using Wirehome.Sensors;
using Wirehome.Areas;
using Wirehome.Extensions.Contracts;
using Wirehome.Contracts.Messaging;
using Wirehome.Contracts.Scheduling;
using Wirehome.Extensions.Messaging;
using Wirehome.Extensions.Messaging.Services;

namespace Wirehome.Controller.Dnf.Rooms
{
    internal partial class TestConfiguration
    {
        private readonly IDeviceRegistryService _deviceService;
        private readonly IAreaRegistryService _areaService;
        private readonly SensorFactory _sensorFactory;
        private readonly ActuatorFactory _actuatorFactory;
        private readonly AutomationFactory _automationFactory;
        private readonly IRemoteSocketService _remoteSocketService;
        private readonly ISerialMessagingService _serialService;
        private readonly IMessageBrokerService _messageBroker;
        private readonly ISchedulerService _schedulerService;

        private const int TIME_TO_ON = 30;
        private const int TIME_WHILE_ON = 5;
        //private Speaker _speaker;
        
        public TestConfiguration(IDeviceRegistryService deviceService, 
                                    IAreaRegistryService areaService,
                                    SensorFactory sensorFactory,
                                    ActuatorFactory actuatorFactory,
                                    AutomationFactory automationFactory,
                                    IRemoteSocketService remoteSocketService,
                                    ISerialMessagingService serialService, 
                                    IMessageBrokerService messageBroker,
                                    ISchedulerService schedulerService
                                    ) 
        {
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
            _areaService = areaService ?? throw new ArgumentNullException(nameof(areaService));
            _actuatorFactory = actuatorFactory ?? throw new ArgumentNullException(nameof(actuatorFactory));
            _sensorFactory = sensorFactory ?? throw new ArgumentNullException(nameof(sensorFactory));
            _automationFactory = automationFactory ?? throw new ArgumentNullException(nameof(automationFactory));
            _remoteSocketService = remoteSocketService ?? throw new ArgumentNullException(nameof(remoteSocketService));
            _serialService = serialService ?? throw new ArgumentNullException(nameof(serialService));
            _messageBroker = messageBroker ?? throw new ArgumentNullException(nameof(messageBroker));
            _schedulerService = schedulerService ?? throw new ArgumentNullException(nameof(schedulerService));
        }

        public void Apply()
        { 
            var room = _areaService.RegisterArea(Room.Test);
           
            var codePair = DipswitchCodeProvider.GetCodePair(DipswitchSystemCode.AllOn, DipswitchUnitCode.D, 3);
            //var socket = _actuatorFactory.RegisterSocket(room, BalconyElements.RemoteSocket, _remoteSocketService.RegisterRemoteSocket(BalconyElements.RemoteSocket.ToString(), "RemoteSocketBridge", codePair));



            //var livingRoomAutomation = _automationFactory.RegisterTurnOnAndOffAutomation(room, LivingroomElements.SchedulerAutomation)
            //.WithSchedulerTime(new AutomationScheduler
            //{
            //    StartTime = new TimeSpan(18, 0, 0),
            //    TurnOnInterval = new TimeSpan(0, 0, 20),
            //    WorkingTime = new TimeSpan(0, 0, 5)
            //})
            //.WithTarget(socket);

            //TEST

            //_schedulerService.Register("TEST_IR", TimeSpan.FromSeconds(3), () =>
            //{
            //    _messageBroker.Publish(typeof(I2CMessagingService).Name, new InfraredMessage
            //    {
            //        IfraredSystem = IfraredSystem.NECX,
            //        Bits = 32,
            //        Code = 3772833823
            //    });
            //});

            //var last = false;

            //_schedulerService.Register("TEST_LPD", TimeSpan.FromSeconds(3), () =>
            //{
            //    _messageBroker.Publish(typeof(I2CMessagingService).Name, new LPD433Message
            //    {
            //        Pin = 7,
            //        Code = last ? codePair.OnCode.Value : codePair.OffCode.Value
            //    });
            //    last = !last;

            //});

            //_messageBroker.Publish(typeof(I2CService).Name, new CurrentMessage
            //{
            //    Pin = 14
            //});

            //_messageBroker.Publish(typeof(II2CMessagingService).Name, new TemperatureMessage
            //{
            //    Pin = 13
            //});


            //_messageBroker.Subscribe<LPD433Message>("SerialService", x =>
            //{
            //    if
            //    (
            //        x.Payload.Content.DipswitchCode?.Command == Hardware.RemoteSockets.RemoteSocketCommand.TurnOff
            //        && x.Payload.Content.DipswitchCode?.System == DipswitchSystemCode.AllOn
            //        && x.Payload.Content.DipswitchCode?.Unit == DipswitchUnitCode.A
            //    )
            //    {
            //        Console.WriteLine("!!!!!!!!!!!!!!!!!!!");

            //        //_messageBroker.Publish(typeof(HttpMessagingService).Name, new DenonMessage
            //        //{
            //        //    ParamName = "cmd0",
            //        //    ParamValue = "PutZone_OnOff/OFF",
            //        //    DeviceAddress = "192.168.0.101"
            //        //});
            //    }

            //    if
            //   (
            //       x.Payload.Content.DipswitchCode?.Command == Hardware.RemoteSockets.RemoteSocketCommand.TurnOn &&
            //       x.Payload.Content.DipswitchCode?.System == DipswitchSystemCode.AllOn &&
            //       x.Payload.Content.DipswitchCode?.Unit == DipswitchUnitCode.A
            //   )
            //    {
            //        //_messageBroker.Publish(typeof(HttpMessagingService).Name, new DenonMessage
            //        //{
            //        //    ParamName = "cmd0",
            //        //    ParamValue = "PutZone_OnOff/ON",
            //        //    DeviceAddress = "192.168.0.101"
            //        //});
            //    }

            //    //_messageBroker.Publish(typeof(II2CMessagingService).Name, new LPD433Message
            //    //{
            //    //    Pin = 7,
            //    //    Code = last ? codePair.OnCode.Value : codePair.OffCode.Value

            //    //});

            //    //last = !last;
            //}
            //);

        }


    } 
}
