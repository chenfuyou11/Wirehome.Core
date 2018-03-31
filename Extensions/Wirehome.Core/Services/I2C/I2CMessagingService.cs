using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wirehome.ComponentModel.Messaging;
using Wirehome.Core.EventAggregator;
using Wirehome.Core.Services.Logging;

namespace Wirehome.Core.Services.I2C
{
    //TODO rewrite in new model
    public class I2CMessagingService : II2CMessagingService
    {
        private readonly ILogger _logService;
        private readonly II2CBusService _i2cServiceBus;
        private readonly List<IBinaryMessage> _messageHandlers = new List<IBinaryMessage>();
        private readonly IEventAggregator _eventAggregator;
        private readonly DisposeContainer _disposeContainer = new DisposeContainer();

        public I2CMessagingService(ILogService logService, IEventAggregator eventAggregator, II2CBusService i2CBusService,
            IEnumerable<IBinaryMessage> handlers)
        {
            _logService = logService.CreatePublisher(nameof(I2CMessagingService));
            _i2cServiceBus = i2CBusService;
            _messageHandlers.AddRange(handlers);
            _eventAggregator = eventAggregator;
        }

        public Task Initialize()
        {
            _disposeContainer.Add(_eventAggregator.SubscribeForAsyncResult<IBinaryMessage>(MessageHandler));
            //_bridge = _deviceService.GetDevice<I2CHardwareBridge>();

            //_messageHandlers.ForEach(handler =>
            //{
            //    _messageBroker.Subscribe(new MessageSubscription
            //    {
            //        Id = Guid.NewGuid().ToString(),
            //        PayloadType = handler.GetType().Name,
            //        Topic = typeof(I2CMessagingService).Name,
            //        Callback = MessageHandler
            //    });
            //});

            return Task.CompletedTask;
        }

        public void Dispose() => _disposeContainer.Dispose();

        private async Task<object> MessageHandler(IMessageEnvelope<IBinaryMessage> message)
        {
            //_messageHandlers.ForEach(handler =>
            //{
            //    if (handler.CanSerialize(message.Payload.Type))
            //    {
            //        try
            //        {
            //            var package = handler.Serialize(message.Payload.Content);
            //            _i2cServiceBus.Write(_bridge.Address, package);
            //        }
            //        catch (Exception ex)
            //        {
            //            _logService.Error(ex, $"Handler of type {handler.GetType().Name} failed to process message");
            //        }
            //    }
            //});
            return null;
        }
    }
}