using Wirehome.Controller.Dnf.Enums;
using Wirehome.Actuators;
using Wirehome.Automations;
using Wirehome.Contracts.Areas;
using Wirehome.Contracts.Core;
using Wirehome.Hardware.Drivers.CCTools.Devices;
using Wirehome.Sensors;
using Wirehome.Extensions.Extensions;
using Wirehome.Areas;
using Wirehome.Sensors.MotionDetectors;
using Wirehome.Actuators.Lamps;
using Wirehome.Extensions.Contracts;
using Wirehome.Extensions.Core;
using Wirehome.Contracts.Components;

namespace Wirehome.Controller.Dnf.Rooms
{
    internal partial class LivingroomConfiguration
    {
        private readonly IDeviceRegistryService _deviceService;
        private readonly IAreaRegistryService _areaService;
        private readonly SensorFactory _sensorFactory;
        private readonly ActuatorFactory _actuatorFactory;
        private readonly AutomationFactory _automationFactory;
        private readonly IAlexaDispatcherEndpointService _alexaService;

        public LivingroomConfiguration(IDeviceRegistryService deviceService,
                                    IAreaRegistryService areaService,
                                    SensorFactory sensorFactory,
                                    ActuatorFactory actuatorFactory,
                                    AutomationFactory automationFactory,
                                    IAlexaDispatcherEndpointService alexaService
                                     )
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
            var input = _deviceService.GetDevice<HSPE16InputOnly>(CCToolsDevices.HSPE16_16.ToString());
            var relays = _deviceService.GetDevice<HSREL8>(CCToolsDevices.HSRel8_32.ToString());
            var tempSensor = _deviceService.GetTempSensor((int)LivingroomElements.TempSensor);
            var humiditySensor = _deviceService.GetHumiditySensor((int)LivingroomElements.TempSensor);

            var room = _areaService.RegisterArea(Room.LivingRoom);

            _sensorFactory.RegisterTemperatureSensor(room, LivingroomElements.TempSensor, tempSensor);
            _sensorFactory.RegisterHumiditySensor(room, LivingroomElements.HumiditySensor, humiditySensor);
            var md = _sensorFactory.RegisterMotionDetector(room, LivingroomElements.MotionDetector, input[HSPE16Pin.GPIO0]);

            var lamp1 = _actuatorFactory.RegisterMonostableLamp(room, LivingroomElements.TVLight, new MonostableBinaryOutputAdapter(relays[HSREL8Pin.Relay0], input[HSPE16Pin.GPIO14]));
            var lamp2 = _actuatorFactory.RegisterMonostableLamp(room, LivingroomElements.BedLight, new MonostableBinaryOutputAdapter(relays[HSREL8Pin.Relay1], input[HSPE16Pin.GPIO13]));

            _alexaService.AddConnectedVivices("Light", new IComponent[] { lamp1, lamp2 });

        }

     
    }
}
