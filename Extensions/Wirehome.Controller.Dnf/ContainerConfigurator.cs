using System;
using Wirehome.Contracts.Core;
using Wirehome.Extensions;
using Wirehome.Extensions.Contracts;
using Wirehome.Extensions.Extensions;
using Wirehome.Extensions.Messaging;
using Wirehome.Extensions.Messaging.Services;
using Wirehome.Raspberry;
using Wirehome.Extensions.Messaging.Core;

namespace Wirehome.Controller.Dnf
{
    internal class ContainerConfigurator : IContainerConfigurator
    {
        public void ConfigureContainer(IContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));

            var projectAssemblies = AssemblyHelper.GetProjectAssemblies();

            container.RegisterRaspberryServices();

            container.RegisterServicesInNamespace(projectAssemblies, typeof(HttpMessagingService).Namespace); // all Wirehome.Extensions.Messaging.Services
            container.RegisterSingleton<IAlexaDispatcherService, AlexaDispatcherService>();
            container.RegisterSingleton<IEventAggregator, EventAggregator>();
            container.RegisterCollection<IBinaryMessage>(projectAssemblies);
            container.RegisterCollection<IHttpMessage>(projectAssemblies);
        }
    }

}
