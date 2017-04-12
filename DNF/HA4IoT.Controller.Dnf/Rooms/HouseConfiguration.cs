using HA4IoT.Actuators;
using HA4IoT.Automations;
using HA4IoT.Contracts.Areas;
using HA4IoT.Contracts.Services.System;
using HA4IoT.Sensors;
using HA4IoT.Extensions;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Extensions.Core;

namespace HA4IoT.Controller.Dnf.Rooms
{

    internal partial class HouseConfiguration 
    {
        private readonly IDeviceRegistryService _deviceService;
        private readonly IAreaRegistryService _areaService;
        private readonly SensorFactory _sensorFactory;
        private readonly ActuatorFactory _actuatorFactory;
        private readonly AutomationFactory _automationFactory;
        private readonly IAlexaDispatcherEndpointService _alexaService;
        private readonly IComponentRegistryService _componentService;

        public HouseConfiguration(IDeviceRegistryService deviceService,
                                    IAreaRegistryService areaService,
                                    SensorFactory sensorFactory,
                                    ActuatorFactory actuatorFactory,
                                    AutomationFactory automationFactory,
                                    IAlexaDispatcherEndpointService alexaService,
                                    IComponentRegistryService componentService
        )  
        {
            _deviceService = deviceService;
            _areaService = areaService;
            _sensorFactory = sensorFactory;
            _actuatorFactory = actuatorFactory;
            _automationFactory = automationFactory;
            _alexaService = alexaService;
            _componentService = componentService;
        }

        public void Apply()
        {

            var all_lamps = _componentService.GetComponents<ILamp>();

            _alexaService.AddConnectedVivices("All lights", all_lamps);
        }
    }
}
