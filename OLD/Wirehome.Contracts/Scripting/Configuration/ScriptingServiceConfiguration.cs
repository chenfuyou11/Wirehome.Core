using System.Collections.Generic;
using Wirehome.Contracts.Configuration;

namespace Wirehome.Contracts.Scripting.Configuration
{
    public class ScriptingServiceConfiguration
    {
        public List<StartupScriptConfiguration> StartupScripts { get; set; } = new List<StartupScriptConfiguration>();
    }
}
