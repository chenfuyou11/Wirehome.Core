using System;
using System.Linq;
using System.Reflection;
using Wirehome.Contracts.Core;
using Wirehome.Extensions;
using Wirehome.Extensions.Contracts;
using Wirehome.Extensions.Extensions;
using Wirehome.Extensions.Messaging;
using Wirehome.Extensions.Messaging.Services;
using Wirehome.Raspberry;
using Wirehome.HttpServer;
using Wirehome.Extensions.Messaging.Core;

namespace Wirehome.Controller.Dnf
{
    internal class ContainerConfigurator : IContainerConfigurator
    {
        public void ConfigureContainer(IContainer containerService)
        {
            if (containerService == null) throw new ArgumentNullException(nameof(containerService));

            containerService.RegisterRaspberryServices();
            containerService.RegisterHttpServer();

            // register all Wirehome.Extensions.Messaging.Services
            var baseTypeForRegister = typeof(HttpMessagingService);
            var extensionsAssemblie = new[] { baseTypeForRegister.GetTypeInfo().Assembly };

            containerService.RegisterSingleton<IAlexaDispatcherEndpointService, AlexaDispatcherEndpointService>();
            containerService.RegisterSingleton<IEventAggregator, EventAggregator>();
            containerService.RegisterServicesInNamespace(extensionsAssemblie.FirstOrDefault(), baseTypeForRegister.Namespace);
            
            containerService.RegisterCollection<IBinaryMessage>(extensionsAssemblie);
            containerService.RegisterCollection<IHttpMessage>(extensionsAssemblie);
        }
    }

}
