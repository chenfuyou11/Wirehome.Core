using Wirehome.Contracts.Core;
using Wirehome.Contracts.Logging;
using Wirehome.Contracts.Messaging;
using Wirehome.Extensions.Contracts;
using Wirehome.Hardware.Drivers.I2CHardwareBridge;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wirehome.Extensions.Messaging.Services
{
    public class I2CMessagingService : II2CMessagingService
    {
        private readonly ILogger _logService;
        private readonly IMessageBrokerService _messageBroker;
        private readonly II2CBusService _i2cServiceBus;
        private readonly IDeviceRegistryService _deviceService;
        private I2CHardwareBridge _bridge;
        private readonly List<IBinaryMessage> _messageHandlers = new List<IBinaryMessage>();

        public I2CMessagingService(ILogService logService, IMessageBrokerService messageBroker, II2CBusService i2CBusService, IDeviceRegistryService deviceService, IEnumerable<IBinaryMessage> handlers)
        {
            _logService = logService.CreatePublisher(nameof(I2CMessagingService));
            _messageBroker = messageBroker;
            _i2cServiceBus = i2CBusService;
            _deviceService = deviceService;
            _messageHandlers.AddRange(handlers);
        }

        public Task Initialize()
        {
            _bridge = _deviceService.GetDevice<I2CHardwareBridge>();
   
            _messageHandlers.ForEach(handler =>
            {
                _messageBroker.Subscribe(new MessageSubscription
                {
                    Id = Guid.NewGuid().ToString(),
                    PayloadType = handler.GetType().Name,
                    Topic = typeof(I2CMessagingService).Name,
                    Callback = MessageHandler
                });
            });

            return Task.CompletedTask;
        }

        private void MessageHandler(Message<JObject> message)
        {
            _messageHandlers.ForEach(handler =>
            {
                if(handler.CanSerialize(message.Payload.Type))
                {
                    try
                    {
                        var package = handler.Serialize(message.Payload.Content);
                        _i2cServiceBus.Write(_bridge.Address, package);
                    }
                    catch(Exception ex)
                    {
                        _logService.Error(ex, $"Handler of type {handler.GetType().Name} failed to process message");
                    }
                }
            });
        }
    }
}
