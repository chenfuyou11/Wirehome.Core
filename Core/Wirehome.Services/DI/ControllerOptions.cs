using System;
using System.ComponentModel;

namespace Wirehome.Core.Services.DependencyInjection
{
    public class ControllerOptions
    {
        public string AdapterRepository { get; set; }
        public string ConfigurationPath { get; set; }
        public Action<IContainer> NativeServicesRegistration { get; set; }
    }
}
