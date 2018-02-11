using Wirehome.Actuators;
using Wirehome.Automations;
using Wirehome.Contracts.Areas;
using Wirehome.Contracts.Core;
using Wirehome.Sensors;
using Wirehome.Extensions.Contracts;
using Wirehome.Contracts.Components;
using Wirehome.Contracts.Actuators;

namespace Wirehome.Controller.Dnf.Rooms
{

    internal partial class HouseConfiguration 
    {
        private readonly IDeviceRegistryService _deviceService;
        private readonly IAreaRegistryService _areaService;
        private readonly SensorFactory _sensorFactory;
        private readonly ActuatorFactory _actuatorFactory;
        private readonly AutomationFactory _automationFactory;
        private readonly IAlexaDispatcherService _alexaService;
        private readonly IComponentRegistryService _componentService;

        public HouseConfiguration(IDeviceRegistryService deviceService,
                                    IAreaRegistryService areaService,
                                    SensorFactory sensorFactory,
                                    ActuatorFactory actuatorFactory,
                                    AutomationFactory automationFactory,
                                    IAlexaDispatcherService alexaService,
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

            _alexaService.RegisterDevice("All lights", all_lamps);
        }
    }
}
