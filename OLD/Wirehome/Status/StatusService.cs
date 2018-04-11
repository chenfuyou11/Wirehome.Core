using System;
using System.Collections.Generic;
using System.Linq;
using Wirehome.Contracts.Api;
using Wirehome.Contracts.Components;
using Wirehome.Contracts.Components.Features;
using Wirehome.Contracts.Components.States;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Sensors;
using Wirehome.Contracts.Services;
using Wirehome.Contracts.Settings;
using Newtonsoft.Json.Linq;

namespace Wirehome.Status
{
    [ApiServiceClass(typeof(IStatusService))]
    public class StatusService : ServiceBase, IStatusService
    {
        private readonly IComponentRegistryService _componentRegistry;
        private readonly ISettingsService _settingsService;

        public StatusService(IComponentRegistryService componentRegistry, IApiDispatcherService apiService, ISettingsService settingsService)
        {
            if (apiService == null) throw new ArgumentNullException(nameof(apiService));

            _componentRegistry = componentRegistry ?? throw new ArgumentNullException(nameof(componentRegistry));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        }

        [ApiMethod]
        public void GetStatus(IApiCall apiCall)
        {
            apiCall.Result = JObject.FromObject(CollectStatus());
        }

        private Status CollectStatus()
        {
            var status = new Status();

            status.OpenWindows.AddRange(GetOpenWindows());
            status.TiltWindows.AddRange(GetTiltWindows());
            status.ActiveComponents.AddRange(GetComponentStatus());

            return status;
        }

        private List<WindowStatus> GetOpenWindows()
        {
            return _componentRegistry.GetComponents<IWindow>()
                .Where(w => w.GetState().Has(WindowState.Open))
                .Select(w => new WindowStatus { Id = w.Id, Caption = _settingsService.GetComponentSettings<ComponentSettings>(w.Id).Caption }).ToList();
        }

        private List<WindowStatus> GetTiltWindows()
        {
            return _componentRegistry.GetComponents<IWindow>()
                .Where(w => w.GetState().Has(WindowState.TildOpen))
                .Select(w => new WindowStatus { Id = w.Id, Caption = _settingsService.GetComponentSettings<ComponentSettings>(w.Id).Caption }).ToList();
        }

        private List<ComponentStatus> GetComponentStatus()
        {
            var actuatorStatusList = new List<ComponentStatus>();

            var components = _componentRegistry.GetComponents();
            foreach (var component in components)
            {
                if (!component.GetFeatures().Supports<PowerStateFeature>())
                {
                    continue;
                }

                if (component.GetState().Has(PowerState.Off))
                {
                    continue;
                }

                var settings = _settingsService.GetComponentSettings<ComponentSettings>(component.Id);
                var actuatorStatus = new ComponentStatus { Id = component.Id, Caption = settings.Caption };
                actuatorStatusList.Add(actuatorStatus);
            }

            return actuatorStatusList;
        }
    }
}
