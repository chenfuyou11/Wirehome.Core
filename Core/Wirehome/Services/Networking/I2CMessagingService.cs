using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wirehome.Core.EventAggregator;
using Wirehome.Core.Interface.Messaging;
using Wirehome.Core.Services.I2C;
using Wirehome.Core.Services.Logging;

namespace Wirehome.Core.Services
{
    public class I2CMessagingService : II2CMessagingService
    {
        private readonly ILogger _logService;
        private readonly II2CBusService _i2cServiceBus;
        private readonly IEventAggregator _eventAggregator;
        private readonly DisposeContainer _disposableContainer = new DisposeContainer();

        public void Dispose() => _disposableContainer.Dispose();

        public I2CMessagingService(ILogService logService, II2CBusService i2CBusService, IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _logService = logService.CreatePublisher(nameof(I2CMessagingService));
            _i2cServiceBus = i2CBusService;
        }

        public Task Initialize()
        {
            _disposableContainer.Add(_eventAggregator.SubscribeAsync<IBinaryMessage>(MessageHandler, RoutingFilter.MessageWrite));
            return Task.CompletedTask;
        }

        private Task MessageHandler(IMessageEnvelope<IBinaryMessage> message)
        {
            try
            {
                _i2cServiceBus.Write(I2CSlaveAddress.FromValue(message.Message.GetAddress()), message.Message.Serialize());
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"Handler of type {message.Message.GetType().Name} failed to process message");
            }
            return Task.CompletedTask;
        }
    }
}
