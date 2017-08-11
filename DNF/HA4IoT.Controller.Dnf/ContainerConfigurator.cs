using HA4IoT.Contracts.Core;
using HA4IoT.Extensions;
using HA4IoT.Extensions.Contracts;
using HA4IoT.Extensions.MessagesModel.Services;
using HA4IoT.Extensions.Messaging;
using HA4IoT.Extensions.Messaging.Services;
using System;
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
            
            containerService.RegisterSingleton<IAlexaDispatcherEndpointService, AlexaDispatcherEndpointService>();
            containerService.RegisterSingleton<ISerialMessagingService, SerialMessagingService>();
            containerService.RegisterSingleton<II2CMessagingService, I2CMessagingService>();
            containerService.RegisterSingleton<IHttpMessagingService, HttpMessagingService>();
            containerService.RegisterSingleton<ITcpMessagingService, TcpMessagingService>();
            
            var assembly = typeof(InfraredMessage).GetTypeInfo().Assembly;
            containerService.RegisterSingletonCollection<IBinaryMessage>(new[] { assembly });
            containerService.RegisterSingletonCollection<IHttpMessage>(new[] { assembly });
        }
    }

}
