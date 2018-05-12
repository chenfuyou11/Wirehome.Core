using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Wirehome.Core.Services.Logging;

namespace Wirehome.Core.Services.DependencyInjection
{
    public class ControllerOptions
    {
        public Type ConfigurationType { get; set; }

        public IContainerConfigurator ContainerConfigurator { get; set; }

        public ICollection<ILogAdapter> LogAdapters { get; } = new Collection<ILogAdapter>();

        public ICollection<IService> CustomServices { get; } = new Collection<IService>();

        public string AdapterRepository { get; set; }
    }
}
