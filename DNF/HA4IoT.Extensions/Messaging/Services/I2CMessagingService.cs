using HA4IoT.Contracts.Core;
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Messaging;
using HA4IoT.Extensions.Contracts;
using HA4IoT.Hardware.Drivers.I2CHardwareBridge;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace HA4IoT.Extensions.Messaging.Services
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

        public void Startup()
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
