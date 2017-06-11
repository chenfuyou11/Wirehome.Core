using HA4IoT.Contracts.Core;
using System.Threading.Tasks;
using HA4IoT.Controller.Dnf.Rooms;
using HA4IoT.Controller.Dnf.Enums;
using HA4IoT.Contracts.Hardware.I2C;
using System;
using HA4IoT.ExternalServices.OpenWeatherMap;
using HA4IoT.Hardware.Interrupts;
using HA4IoT.Contracts.Hardware.RaspberryPi;
using HA4IoT.Hardware.RemoteSockets;
using HA4IoT.Hardware.Drivers.CCTools;
using HA4IoT.Contracts.Scheduling;
using HA4IoT.Hardware.Drivers.I2CHardwareBridge;

namespace HA4IoT.Controller.Dnf
{
    internal class Configuration : IConfiguration
    {
        //private const byte ARDUINO_433_READ_PIN = 1;
        //private const byte ARDUINO_433_SEND_PIN = 7;

        private readonly InterruptMonitorService _interruptMonitorService;
        private readonly CCToolsDeviceService _ccToolsBoardService;
        private readonly IGpioService _gpioService;
        private readonly IDeviceRegistryService _deviceService;
        private readonly II2CBusService _i2CBusService;
        private readonly ISchedulerService _schedulerService;
        private readonly RemoteSocketService _remoteSocketService;
        private readonly IContainer _containerService;
        private readonly OpenWeatherMapService _weatherService;

        public Configuration(
            CCToolsDeviceService ccToolsBoardService,
            IGpioService gpioService,
            IDeviceRegistryService deviceService,
            II2CBusService i2CBusService,
            ISchedulerService schedulerService,
            RemoteSocketService remoteSocketService,
            InterruptMonitorService interruptMonitorService,
            IContainer containerService,
            OpenWeatherMapService weatherService
            )
        {
            _interruptMonitorService = interruptMonitorService ?? throw new ArgumentNullException(nameof(interruptMonitorService));
            _ccToolsBoardService = ccToolsBoardService ?? throw new ArgumentNullException(nameof(ccToolsBoardService));
            _gpioService = gpioService ?? throw new ArgumentNullException(nameof(gpioService));
            _deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
            _i2CBusService = i2CBusService ?? throw new ArgumentNullException(nameof(i2CBusService));
            _schedulerService = schedulerService ?? throw new ArgumentNullException(nameof(schedulerService));
            _remoteSocketService = remoteSocketService ?? throw new ArgumentNullException(nameof(remoteSocketService));
            _containerService = containerService ?? throw new ArgumentNullException(nameof(containerService));
            _weatherService = weatherService ?? throw new ArgumentNullException(nameof(weatherService));
        }

        public Task ApplyAsync()
        {
            _weatherService.Settings.AppId = "bdff167243cc14c420b941ddc7eda50d";
            _weatherService.Settings.Latitude = 51.756757f;
            _weatherService.Settings.Longitude = 19.525681f;
            _weatherService.Refresh(null);

            _containerService.GetInstance<LivingroomConfiguration>().Apply();
            _containerService.GetInstance<BalconyConfiguration>().Apply();
            _containerService.GetInstance<BedroomConfiguration>().Apply();
            _containerService.GetInstance<BathroomConfiguration>().Apply();
            _containerService.GetInstance<ToiletConfiguration>().Apply();
            _containerService.GetInstance<KitchenConfiguration>().Apply();
            _containerService.GetInstance<HallwayConfiguration>().Apply();
            _containerService.GetInstance<HouseConfiguration>().Apply();
            _containerService.GetInstance<StaircaseConfiguration>().Apply();

            return Task.FromResult(0);
        }


    }
}
