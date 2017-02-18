using HA4IoT.Actuators;
using HA4IoT.Automations;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Hardware.CCTools;
using HA4IoT.Hardware.I2CHardwareBridge;
using HA4IoT.Sensors;
using HA4IoT.Services.Areas;
using HA4IoT.Services.Devices;
using System.Diagnostics;
using HA4IoT.Controller.Dnf.Enums;
using HA4IoT.Sensors.MotionDetectors;
using HA4IoT.Actuators.Lamps;
using HA4IoT.Contracts.Components;
using HA4IoT.Actuators.BinaryStateActuators;
using HA4IoT.Extensions;
using HA4IoT.Contracts.Actuators;

namespace HA4IoT.Controller.Dnf.Rooms
{

    internal partial class HouseConfiguration 
    {
        private readonly IDeviceService _deviceService;
        private readonly IAreaService _areaService;
        private readonly SensorFactory _sensorFactory;
        private readonly ActuatorFactory _actuatorFactory;
        private readonly AutomationFactory _automationFactory;
        private readonly IComponentService _componentService;
        private readonly IAlexaDispatcherEndpointService _alexaService;

        public HouseConfiguration(IDeviceService deviceService,
                                    IAreaService areaService,
                                    SensorFactory sensorFactory,
                                    ActuatorFactory actuatorFactory,
                                    AutomationFactory automationFactory,
                                    IComponentService componentService,
                                    IAlexaDispatcherEndpointService alexaService)  
        {
            _deviceService = deviceService;
            _areaService = areaService;
            _sensorFactory = sensorFactory;
            _actuatorFactory = actuatorFactory;
            _automationFactory = automationFactory;
            _componentService = componentService;
            _alexaService = alexaService;
        }

        public void Apply()
        {
            var all_lamps = _componentService.GetComponents<IMonostableLamp>();

            _alexaService.AddConnectedVivices("All lights", all_lamps);
        }
    }
}
