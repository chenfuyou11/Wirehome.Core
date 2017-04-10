using System;
using HA4IoT.Contracts.Core;
using HA4IoT.Contracts;
using System.Collections.Generic;

namespace HA4IoT.Core
{
    public class ControllerOptions
    {
        public int HttpServerPort { get; set; } = 80;

        public int? StatusLedGpio { get; set; }

        public Type ConfigurationType { get; set; }

        public IContainerConfigurator ContainerConfigurator { get; set; }
    }
}
