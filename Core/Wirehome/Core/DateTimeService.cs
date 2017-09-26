using System;
using Wirehome.Contracts.Api;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Scripting;
using Wirehome.Contracts.Services;
using Newtonsoft.Json.Linq;

namespace Wirehome.Core
{
    [ApiServiceClass(typeof(IDateTimeService))]
    public class DateTimeService : ServiceBase, IDateTimeService
    {
        public DateTimeService(IScriptingService scriptingService)
        {
            if (scriptingService == null) throw new ArgumentNullException(nameof(scriptingService));

            scriptingService.RegisterScriptProxy(s => new DateTimeScriptProxy(this));
        }

        public DateTime Date => DateTime.Now.Date;

        public TimeSpan Time => DateTime.Now.TimeOfDay;
    
        public DateTime Now => DateTime.Now;
    
        [ApiMethod]
        public void Status(IApiCall apiCall)
        {
            apiCall.Result = JObject.FromObject(this);
        }
    }
}
