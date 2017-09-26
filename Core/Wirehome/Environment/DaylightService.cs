using System;
using Wirehome.Contracts.Api;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Environment;
using Wirehome.Contracts.Scripting;
using Wirehome.Contracts.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Wirehome.Environment
{
    [ApiServiceClass(typeof(IDaylightService))]
    public class DaylightService : ServiceBase, IDaylightService
    {
        private readonly IDateTimeService _dateTimeService;

        public DaylightService(IDateTimeService dateTimeService, IApiDispatcherService apiService, IScriptingService scriptingService)
        {
            if (scriptingService == null) throw new ArgumentNullException(nameof(scriptingService));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));

            apiService.StatusRequested += (s, e) =>
            {
                e.ApiContext.Result.Merge(JObject.FromObject(this));
            };

            scriptingService.RegisterScriptProxy(s => new DaylightScriptProxy(this, dateTimeService));
        }

        public TimeSpan Sunrise { get; private set; } = TimeSpan.Parse("06:45");
        public TimeSpan Sunset { get; private set; } = TimeSpan.Parse("20:30");

        [JsonIgnore]
        public DateTime? Timestamp { get; private set; }

        [ApiMethod]
        public void GetStatus(IApiCall apiCall)
        {
            apiCall.Result = JObject.FromObject(this);
        }

        public void Update(TimeSpan sunrise, TimeSpan sunset)
        {
            Sunrise = sunrise;
            Sunset = sunset;
            Timestamp = _dateTimeService.Now;
        }
    }
}
