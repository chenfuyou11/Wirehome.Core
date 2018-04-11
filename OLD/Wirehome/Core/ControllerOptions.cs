using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Logging;
using Wirehome.Contracts.Services;

namespace Wirehome.Core
{
    public class ControllerOptions
    {
        public Type ConfigurationType { get; set; }

        public IContainerConfigurator ContainerConfigurator { get; set; }

        public ICollection<ILogAdapter> LogAdapters { get; } = new Collection<ILogAdapter>();

        public ICollection<IService> CustomServices { get; } = new Collection<IService>();
    }
}
