using HA4IoT.Contracts.Core;
using System.Threading.Tasks;
using HA4IoT.Controller.Dnf.Rooms;
using System;
using HA4IoT.ExternalServices.OpenWeatherMap;

namespace HA4IoT.Controller.Dnf
{
    internal class Configuration : IConfiguration
    {
        private readonly IContainer _containerService;
        private readonly OpenWeatherMapService _weatherService;

        public Configuration(

            IContainer containerService,
            OpenWeatherMapService weatherService
            )
        {
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
