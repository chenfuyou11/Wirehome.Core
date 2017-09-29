using System;
using Wirehome.Contracts.Environment;
using Newtonsoft.Json.Linq;

namespace Wirehome.ExternalServices.OpenWeatherMap
{
    public class OpenWeatherMapResponseParser
    {
        public float Temperature { get; private set; }

        public float Humidity { get; private set; }

        public TimeSpan Sunrise { get; private set; }

        public TimeSpan Sunset { get; private set; }

        public WeatherCondition Condition { get; private set; }

        public int ConditionCode { get; private set; }

        public void Parse(string source)
        {
            //TODO Check
            var data = JObject.Parse(source);

            var main = data["main"];
            Temperature = (float)main["temp"];
            Humidity = (float)main["humidity"];

            var sys = data["sys"];
            var sunriseValue = (float)sys["sunrise"];
            var sunsetValue = (float)sys["sunset"];
            Sunrise = UnixTimeStampToDateTime(sunriseValue).TimeOfDay;
            Sunset = UnixTimeStampToDateTime(sunsetValue).TimeOfDay;

            var weather = data["weather"];
            ConditionCode = (int)weather["id"];
            Condition = OpenWeatherMapWeatherConditionParser.Parse(ConditionCode);
        }

        private static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            var buffer = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return buffer.AddSeconds(unixTimeStamp).ToLocalTime();
        }
    }
}
