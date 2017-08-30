using HA4IoT.Contracts.Core;
using HA4IoT.Extensions;
using HA4IoT.Extensions.Contracts;
using HA4IoT.Extensions.Extensions;
using HA4IoT.Extensions.Messaging;
using HA4IoT.Extensions.Messaging.Services;
using System;
using System.Linq;
using System.Reflection;

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

            var baseTypeForRegister = typeof(SerialMessagingService);
            var extensionsAssemblie = new[] { baseTypeForRegister.GetTypeInfo().Assembly };

            containerService.RegisterSingleton<IAlexaDispatcherEndpointService, AlexaDispatcherEndpointService>();
            containerService.RegisterServicesInNamespace(extensionsAssemblie.FirstOrDefault(), baseTypeForRegister.Namespace);
            containerService.RegisterSingletonCollection<IBinaryMessage>(extensionsAssemblie);
            containerService.RegisterSingletonCollection<IHttpMessage>(extensionsAssemblie);
        }

        
    }

}
