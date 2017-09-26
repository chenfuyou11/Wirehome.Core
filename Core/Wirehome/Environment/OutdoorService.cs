using System;
using Wirehome.Contracts.Api;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Environment;
using Wirehome.Contracts.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Wirehome.Environment
{
    [ApiServiceClass(typeof(IOutdoorService))]
    public class OutdoorService : ServiceBase, IOutdoorService
    {
        private readonly IDateTimeService _dateTimeService;

        public OutdoorService(IDateTimeService dateTimeService, IApiDispatcherService apiService)
        {
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));

            apiService.StatusRequested += (s, e) =>
            {
                e.ApiContext.Result.Merge(JObject.FromObject(this));
            };
        }

        public float Humidity { get; private set; }

        public DateTime? HumidityTimestamp { get; private set; }

        public float Temperature { get; private set; }

        public DateTime? TemperatureTimestamp { get; private set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public WeatherCondition Condition { get; private set; }

        public DateTime? ConditionTimestamp { get; private set; }

        [ApiMethod]
        public void GetStatus(IApiCall apiCall)
        {
            apiCall.Result = JObject.FromObject(this);
        }

        public void UpdateTemperature(float value)
        {
            Temperature = (float)Math.Round(Convert.ToDouble(value), 1);
            TemperatureTimestamp = _dateTimeService.Now;
        }

        public void UpdateHumidity(float value)
        {
            Humidity = (float)Math.Round(Convert.ToDouble(value), 0);
            HumidityTimestamp = _dateTimeService.Now;
        }

        public void UpdateCondition(WeatherCondition value)
        {
            Condition = value;
            ConditionTimestamp = _dateTimeService.Now;
        }
    }
}
