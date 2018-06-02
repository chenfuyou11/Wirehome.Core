using System;
using System.ComponentModel;

namespace Wirehome.Core.Services.DependencyInjection
{
    public class ControllerOptions
    {
        public Action<IContainer> NativeServicesRegistration { get; set; }
        public Action<IContainer> BaseServicesRegistration { get; set; }
        public AdapterMode AdapterMode { get; set; }
    }
}
