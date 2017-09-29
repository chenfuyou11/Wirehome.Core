using System;
using System.Linq;
using System.Reflection;
using Wirehome.Contracts.Core;
using Wirehome.Extensions;
using Wirehome.Extensions.Contracts;
using Wirehome.Extensions.Extensions;
using Wirehome.Raspberry;

namespace Wirehome.Controller.Dnf
{
    internal class ContainerConfigurator : IContainerConfigurator
    {
        public ContainerConfigurator()
        {
        }

        public void ConfigureContainer(IContainer containerService)
        {
            if (containerService == null) throw new ArgumentNullException(nameof(containerService));

            containerService.RegisterRaspberryServices();

            var baseTypeForRegister = typeof(AlexaDispatcherEndpointService);
            var extensionsAssemblie = new[] { baseTypeForRegister.GetTypeInfo().Assembly };

            containerService.RegisterSingleton<IAlexaDispatcherEndpointService, AlexaDispatcherEndpointService>();
            containerService.RegisterServicesInNamespace(extensionsAssemblie.FirstOrDefault(), baseTypeForRegister.Namespace);
            
            //TODO
            //containerService.RegisterSingletonCollection<IBinaryMessage>(extensionsAssemblie);
            //containerService.RegisterSingletonCollection<IHttpMessage>(extensionsAssemblie);
        }

        
    }

}
