using System;
using Wirehome.Contracts.Environment;
using Wirehome.Contracts.Services;

namespace Wirehome.Tests.Mockups
{
    public class TestWeatherStation : ServiceBase, IOutdoorService
    {
        public WeatherCondition Condition { get; set; }
        public DateTime? ConditionTimestamp { get; set; }
        public float Humidity { get; set; }
        public DateTime? HumidityTimestamp { get; set; }
        public float Temperature { get; set; }
        public DateTime? TemperatureTimestamp { get; set;  }

        public void UpdateHumidity(float value)
        {
            Humidity = value;
            HumidityTimestamp = DateTime.Now;
        }

        public void UpdateTemperature(float value)
        {
            Temperature = value;
            TemperatureTimestamp = DateTime.Now;
        }

        public void UpdateCondition(WeatherCondition value)
        {
            Condition = value;
            ConditionTimestamp = DateTime.Now;
        }
    }
}
