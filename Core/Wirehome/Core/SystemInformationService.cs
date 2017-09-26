using System;
using System.Collections.Generic;
using Wirehome.Contracts.Api;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Scripting;
using Wirehome.Contracts.Services;
using Newtonsoft.Json.Linq;

namespace Wirehome.Core
{
    [ApiServiceClass(typeof(ISystemInformationService))]
    public class SystemInformationService : ServiceBase, ISystemInformationService
    {
        private readonly Dictionary<string, Func<object>> _values = new Dictionary<string, Func<object>>();

        public SystemInformationService(IScriptingService scriptingService)
        {
            if (scriptingService == null) throw new ArgumentNullException(nameof(scriptingService));

            scriptingService.RegisterScriptProxy(s => new SystemInformationScriptProxy(this));
        }

        public void Set(string key, object value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            lock (_values)
            {
                _values[key] = () => value;
            }
        }

        public void Set(string key, Func<object> valueProvider)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (valueProvider == null) throw new ArgumentNullException(nameof(valueProvider));

            lock (_values)
            {
                _values[key] = valueProvider;
            }
        }

        public void Delete(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            lock (_values)
            {
                _values.Remove(key);
            }
        }

        [ApiMethod]
        public void GetStatus(IApiCall apiCall)
        {
            Dictionary<string, Func<object>> values;
            lock (_values)
            {
                values = new Dictionary<string, Func<object>>(_values);
            }

            var json = new JObject();
            foreach (var value in values)
            {
                var effectiveValue = value.Value();
                if (effectiveValue == null)
                {
                    json[value.Key] = JValue.CreateNull();
                }
                else
                {
                    json[value.Key] = JToken.FromObject(value.Value());
                }
            }

            apiCall.Result = json;
        }
    }
}
