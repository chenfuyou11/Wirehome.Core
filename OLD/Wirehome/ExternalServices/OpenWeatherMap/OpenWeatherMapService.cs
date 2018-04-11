using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Wirehome.Contracts.Api;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Environment;
using Wirehome.Contracts.Logging;
using Wirehome.Contracts.Scheduling;
using Wirehome.Contracts.Services;
using Wirehome.Contracts.Settings;
using Wirehome.Contracts.Storage;
using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace Wirehome.ExternalServices.OpenWeatherMap
{
    [ApiServiceClass(typeof(OpenWeatherMapService))]
    public class OpenWeatherMapService : ServiceBase
    {
        private readonly IOutdoorService _outdoorService;
        private readonly IDaylightService _daylightService;
        private readonly IDateTimeService _dateTimeService;
        private readonly ISystemInformationService _systemInformationService;
        private readonly ILogger _log;
        private readonly ISettingsService _settingsService;
        private readonly ISchedulerService _schedulerService;

        public float Temperature { get; private set; }
        public float Humidity { get; private set; }
        public TimeSpan Sunrise { get; private set; }
        public TimeSpan Sunset { get; private set; }
        public WeatherCondition Condition { get; private set; }
        
        public OpenWeatherMapService(
            IOutdoorService outdoorService,
            IDaylightService daylightService,
            IDateTimeService dateTimeService, 
            ISchedulerService schedulerService, 
            ISystemInformationService systemInformationService,
            ISettingsService settingsService,
            ILogService logService)
        {
            _outdoorService = outdoorService ?? throw new ArgumentNullException(nameof(outdoorService));
            _daylightService = daylightService ?? throw new ArgumentNullException(nameof(daylightService));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
            _systemInformationService = systemInformationService ?? throw new ArgumentNullException(nameof(systemInformationService));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _log = logService?.CreatePublisher(nameof(OpenWeatherMapService)) ?? throw new ArgumentNullException(nameof(logService));
            _schedulerService = schedulerService ?? throw new ArgumentNullException(nameof(schedulerService));
        }

        public OpenWeatherMapServiceSettings Settings { get; private set; }


        public override Task Initialize()
        {
            //TODO Moved to Init
            _settingsService.CreateSettingsMonitor<OpenWeatherMapServiceSettings>(s => Settings = s.NewSettings);
            _schedulerService.Register("OpenWeatherMapServiceUpdater", TimeSpan.FromMinutes(5), RefreshAsync);

            return Task.CompletedTask;
        }

        [ApiMethod]
        public void Status(IApiCall apiCall)
        {
            apiCall.Result = JObject.FromObject(this);
        }

        [ApiMethod]
        public void Refresh(IApiCall apiCall)
        {
            RefreshAsync().Wait();
        }

        private async Task RefreshAsync()
        {
            if (!Settings.IsEnabled)
            {
                _log.Verbose("OpenWeatherMapService is disabled.");
                return;
            }

            _log.Verbose("Fetching OpenWeatherMap data.");

            var response = await FetchWeatherDataAsync();
            if (TryParseData(response))
            {
                PushData();
            }

            _systemInformationService.Set("OpenWeatherMapService/Timestamp", _dateTimeService.Now);
        }

        private void PushData()
        {
            if (Settings.UseTemperature)
            {
                _outdoorService.UpdateTemperature(Temperature);
            }

            if (Settings.UseHumidity)
            {
                _outdoorService.UpdateHumidity(Humidity);
            }

            if (Settings.UseSunriseSunset)
            {
                _daylightService.Update(Sunrise, Sunset);
            }

            if (Settings.UseWeather)
            {
                _outdoorService.UpdateCondition(Condition);
            }
        }

        private async Task<string> FetchWeatherDataAsync()
        {
            var uri = new Uri($"http://api.openweathermap.org/data/2.5/weather?lat={Settings.Latitude}&lon={Settings.Longitude}&APPID={Settings.AppId}&units=metric");

            _systemInformationService.Set($"{nameof(OpenWeatherMapService)}/Uri", uri.ToString());

            var stopwatch = Stopwatch.StartNew();
            try
            {
                using (var httpClient = new HttpClient())
                using (var result = await httpClient.GetAsync(uri))
                {
                    return await result.Content.ReadAsStringAsync();
                }
            }
            finally
            {
                _systemInformationService.Set($"{nameof(OpenWeatherMapService)}/LastFetchDuration", stopwatch.Elapsed);
            }
        }

        private bool TryParseData(string weatherData)
        {
            try
            {
                var parser = new OpenWeatherMapResponseParser();
                parser.Parse(weatherData);

                _systemInformationService.Set($"{nameof(OpenWeatherMapService)}/LastConditionCode", parser.ConditionCode);

                Condition = parser.Condition;
                Temperature = parser.Temperature;
                Humidity = parser.Humidity;

                Sunrise = parser.Sunrise;
                Sunset = parser.Sunset;

                return true;
            }
            catch (Exception exception)
            {
                _log.Warning(exception, $"Error while parsing Open Weather Map response ({weatherData}).");

                return false;
            }
        }
    }
}