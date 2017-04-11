using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Hardware.RemoteSwitch;
using System.Threading.Tasks;
using HA4IoT.Controller.Dnf.Rooms;
using HA4IoT.Hardware;
using HA4IoT.Controller.Dnf.Enums;
using HA4IoT.Contracts;
using HA4IoT.Contracts.Hardware.I2C;
using HA4IoT.Hardware.I2C.I2CHardwareBridge;
using HA4IoT.Contracts.Hardware.Services;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Contracts.Logging;

namespace HA4IoT.Controller.Dnf
{
    internal class Configuration : IConfiguration
    {
        private const byte RASPBERRY_INTERRUPT = 4;

        private const byte ARDUINO_433_READ_PIN = 1;
        private const byte ARDUINO_433_SEND_PIN = 7;

        private const byte I2C_ADDRESS_ARDUINO = 50;
        private const byte I2C_ADDRESS_REL_1 = 32;    // GND - GND - GND (32)
        private const byte I2C_ADDRESS_REL_2 = 24;    // SCL - SCL - GND (24)
        private const byte I2C_ADDRESS_INPUT_1 = 88;  // SCL - SCL - SCL (88)
        private const byte I2C_ADDRESS_INPUT_2 = 16;  // GND - SCL - GND (16)

        private readonly CCToolsDeviceService _ccToolsBoardService;
        private readonly IGpioService _pi2GpioService;
        private readonly IDeviceRegistryService _deviceService;
        private readonly II2CBusService _i2CBusService;
        private readonly ISchedulerService _schedulerService;
        private readonly RemoteSocketService _remoteSocketService;
        private readonly IContainer _containerService;
        private readonly ILogService _logService;

        public Configuration(
            CCToolsDeviceService ccToolsBoardService,
            IGpioService pi2GpioService,
            IDeviceRegistryService deviceService,
            II2CBusService i2CBusService,
            ISchedulerService schedulerService,
            RemoteSocketService remoteSocketService,
            IContainer containerService,
            ILogService logService
            )
        {
            _ccToolsBoardService = ccToolsBoardService;
            _pi2GpioService = pi2GpioService;
            _deviceService = deviceService;
            _i2CBusService = i2CBusService;
            _schedulerService = schedulerService;
            _remoteSocketService = remoteSocketService;
            _containerService = containerService;
            _logService = logService;

        }

        public Task ApplyAsync()
        {
            //_synonymService.TryLoadPersistedSynonyms();

            _ccToolsBoardService.RegisterHSPE16InputOnly(CCToolsDevices.HSPE16_88.ToString(), new I2CSlaveAddress(I2C_ADDRESS_INPUT_1));
            _ccToolsBoardService.RegisterHSPE16InputOnly(CCToolsDevices.HSPE16_16.ToString(), new I2CSlaveAddress(I2C_ADDRESS_INPUT_2));
            _ccToolsBoardService.RegisterHSREL8(CCToolsDevices.HSRel8_32.ToString(), new I2CSlaveAddress(I2C_ADDRESS_REL_1));
            _ccToolsBoardService.RegisterHSREL8(CCToolsDevices.HSRel8_24.ToString(), new I2CSlaveAddress(I2C_ADDRESS_REL_2));

            var i2CHardwareBridge = new I2CHardwareBridge(new I2CSlaveAddress(I2C_ADDRESS_ARDUINO), _i2CBusService, _schedulerService);
            _deviceService.AddDevice(i2CHardwareBridge);

            // TODO
            //var currentController = new CurrentController(i2CHardwareBridge.CurrentAccessor);
            //_deviceService.AddDevice(currentController);

            //_remoteSocketService.Sender = new LPD433MHzSignalSender(i2CHardwareBridge, ARDUINO_433_SEND_PIN, _apiService);
            //var brennenstuhl = new BrennenstuhlCodeSequenceProvider();
            //_remoteSocketService.RegisterRemoteSocket(0, brennenstuhl.GetSequencePair(BrennenstuhlSystemCode.AllOn, BrennenstuhlUnitCode.A));

            _containerService.GetInstance<LivingroomConfiguration>().Apply();
            _containerService.GetInstance<BalconyConfiguration>().Apply();
            _containerService.GetInstance<BedroomConfiguration>().Apply();
            _containerService.GetInstance<BathroomConfiguration>().Apply();
            _containerService.GetInstance<ToiletConfiguration>().Apply();
            _containerService.GetInstance<KitchenConfiguration>().Apply();
            _containerService.GetInstance<HallwayConfiguration>().Apply();
            _containerService.GetInstance<HouseConfiguration>().Apply();
            _containerService.GetInstance<StaircaseConfiguration>().Apply();


            //_synonymService.RegisterDefaultComponentStateSynonyms();

            var ioBoardsInterruptMonitor = new InterruptMonitor(_pi2GpioService.GetInput(RASPBERRY_INTERRUPT), _logService);
            ioBoardsInterruptMonitor.InterruptDetected += (s, e) => _ccToolsBoardService.PollInputBoardStates();
            ioBoardsInterruptMonitor.Start();

            return Task.FromResult(0);
        }


    }
}
