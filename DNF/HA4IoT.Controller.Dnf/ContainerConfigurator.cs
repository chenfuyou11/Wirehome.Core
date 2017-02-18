using HA4IoT.Contracts;
using HA4IoT.Contracts.Core;
using HA4IoT.Extensions;
using System;

namespace HA4IoT.Controller.Dnf
{
    internal class ContainerConfigurator : IContainerConfigurator
    {

        public ContainerConfigurator()
        {
        }

        public void ConfigureContainer(IContainer containerService)
        {
            if (containerService == null) throw new ArgumentNullException(nameof(containerService));

            containerService.RegisterSingleton<IAlexaDispatcherEndpointService, AlexaDispatcherEndpointService>();
        }
    }

}
