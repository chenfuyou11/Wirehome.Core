using System;
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
        public void ConfigureContainer(IContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));

            var projectAssemblies = AssemblyHelper.GetProjectAssemblies();
            var messagingServicesTypes = typeof(HttpMessagingService); // all Wirehome.Extensions.Messaging.Services

            container.RegisterRaspberryServices();
            container.RegisterHttpServer();

            container.RegisterServicesInNamespace(projectAssemblies, messagingServicesTypes.Namespace);
            container.RegisterSingleton<IAlexaDispatcherEndpointService, AlexaDispatcherEndpointService>();
            container.RegisterSingleton<IEventAggregator, EventAggregator>();
            container.RegisterCollection<IBinaryMessage>(projectAssemblies);
            container.RegisterCollection<IHttpMessage>(projectAssemblies);
        }
    }

}
