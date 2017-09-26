using System;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Scripting;
using MoonSharp.Interpreter;

namespace Wirehome.Core
{
    public class SystemInformationScriptProxy : IScriptProxy
    {
        private readonly ISystemInformationService _systemInformationService;

        [MoonSharpHidden]
        public SystemInformationScriptProxy(ISystemInformationService systemInformationService)
        {
            _systemInformationService = systemInformationService ?? throw new ArgumentNullException(nameof(systemInformationService));
        }

        [MoonSharpHidden]
        public string Name => "systemInformation";

        public void Set(string key, object value)
        {
            _systemInformationService.Set(key, value);
        }

        public void Delete(string key)
        {
            _systemInformationService.Delete(key);
        }
    }
}
