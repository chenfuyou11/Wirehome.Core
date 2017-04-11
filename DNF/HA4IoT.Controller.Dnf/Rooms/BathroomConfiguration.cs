using HA4IoT.Actuators;
using HA4IoT.Automations;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Sensors;
using HA4IoT.Controller.Dnf.Enums;
using HA4IoT.Sensors.MotionDetectors;
using HA4IoT.Extensions.Extensions;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Contracts.Areas;
using HA4IoT.Hardware.CCTools;
using System;
using HA4IoT.Hardware.CCTools.Devices;
using HA4IoT.Services.Areas;
using HA4IoT.Extensions.Core;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Contracts.Services.Daylight;
using HA4IoT.Contracts.Services.Settings;

namespace HA4IoT.Controller.Dnf.Rooms
{
    internal partial class BathroomConfiguration 
    {
        private readonly IDeviceRegistryService _deviceService;
        private readonly IAreaRegistryService _areaService;
        private readonly SensorFactory _sensorFactory;
        private readonly ActuatorFactory _actuatorFactory;
        private readonly AutomationFactory _automationFactory;
        private readonly ISchedulerService _schedulerService;
        private readonly IDateTimeService _dateTimeService;
        private readonly IDaylightService _daylightService;
        private readonly ISettingsService _settingsService;

        public BathroomConfiguration(IDeviceRegistryService deviceService,
                                    IAreaRegistryService areaService,
                                    SensorFactory sensorFactory,
                                    ActuatorFactory actuatorFactory,
                                    AutomationFactory automationFactory,
                                    ISchedulerService schedulerService,
                                    IDateTimeService dateTimeService,
                                    IDaylightService daylightService,
                                    ISettingsService settingsService) 
        {
            _deviceService = deviceService;
            _areaService = areaService;
            _sensorFactory = sensorFactory;
            _actuatorFactory = actuatorFactory;
            _automationFactory = automationFactory;
            _schedulerService = schedulerService;
            _dateTimeService = dateTimeService;
            _daylightService = daylightService;
            _settingsService = settingsService;
        }

        public void Apply()
        {
            var input = _deviceService.GetDevice<HSPE16InputOnly>(CCToolsDevices.HSPE16_16.ToString());
            var input_88 = _deviceService.GetDevice<HSPE16InputOnly>(CCToolsDevices.HSPE16_88.ToString());
            var relays = _deviceService.GetDevice<HSREL8>(CCToolsDevices.HSRel8_24.ToString());
            var tempSensor = _deviceService.GetTempSensor((int)BathroomElements.TempSensor);
            var humiditySensor = _deviceService.GetHumiditySensor((int)BathroomElements.TempSensor);

            var room = _areaService.RegisterArea(Room.Bathroom);

            _sensorFactory.RegisterTemperatureSensor(room, BathroomElements.TempSensor, tempSensor);
            _sensorFactory.RegisterHumiditySensor(room, BathroomElements.HumiditySensor, humiditySensor);
            _sensorFactory.RegisterMotionDetector(room, BathroomElements.MotionDetector, input[HSPE16Pin.GPIO2]);
            
            _actuatorFactory.RegisterMonostableLamp(room, BathroomElements.Light, new MonostableBinaryOutputAdapter(relays[HSREL8Pin.Relay0], input_88[HSPE16Pin.GPIO11], _schedulerService));


            var automation =
              new TurnOnAndOffAutomationEx(
                  $"{room.Id}.{BathroomElements.LightAutomation}",
                  _dateTimeService,
                  _schedulerService,
                  _settingsService,
                  _daylightService);

            room.AddAutomation(automation);

              automation
             .WithTrigger(room.GetMotionDetector(BathroomElements.MotionDetector))
             .WithTarget(room.GetLamp(BathroomElements.Light))
             .WithDisableTurnOffWhenBinaryStateEnabled(input_88[HSPE16Pin.GPIO1]);
            
        }

    }
}
