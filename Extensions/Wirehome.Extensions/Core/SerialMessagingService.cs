using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Logging;
using Wirehome.Contracts.Messaging;
using Wirehome.Extensions.Contracts;

namespace Wirehome.Extensions.Messaging.Services
{
    public class SerialMessagingService : ISerialMessagingService
    {        
        private CancellationTokenSource _readCancellationTokenSource = new CancellationTokenSource();
        private IBinaryReader _dataReader;

        private readonly ILogger _logService;
        private readonly INativeSerialDevice _serialDevice;
        private readonly IMessageBrokerService _messageBroker;
        private readonly List<IBinaryMessage> _messageHandlers = new List<IBinaryMessage>();

        public SerialMessagingService(INativeSerialDevice serialDevice, ILogService logService, 
            IMessageBrokerService messageBroker, IEnumerable<IBinaryMessage> handlers)
        {
            _logService = logService.CreatePublisher(nameof(SerialMessagingService));
            _serialDevice = serialDevice ?? throw new ArgumentNullException(nameof(serialDevice));
            _messageBroker = messageBroker;
            _messageHandlers.AddRange(handlers);
        }

        public async Task Initialize()
        {
            await _serialDevice.Init();
            _dataReader = _serialDevice.GetBinaryReader();
            
            Listen();
        }

        private async void Listen()
        {
            try
            {
                while (true)
                {
                    await ReadAsync(_readCancellationTokenSource.Token);
                }
            }
            catch (TaskCanceledException)
            {
                CloseDevice();
            }
            catch (Exception ex)
            {
                _logService.Error(ex.ToString());
            }
            finally
            {
                _dataReader.Dispose();
            }
        }

        // DNF TODO

        //public void RegisterHandler(IBinaryMessage handler)
        //{
        //    _messageHandlers.Add(handler);
        //}

        //public void Close()
        //{
        //    CancelReadTask();
        //    CloseDevice();  
        //}

        private void CloseDevice()
        {
            _serialDevice.Dispose();
        }

        private void CancelReadTask()
        {
            if (_readCancellationTokenSource != null)
            {
                if (!_readCancellationTokenSource.IsCancellationRequested)
                {
                    _readCancellationTokenSource.Cancel();
                }
            }
        }

        private async Task ReadAsync(CancellationToken cancellationToken)
        {
            const uint messageHeaderSize = 2;
            cancellationToken.ThrowIfCancellationRequested();
            
            using (var childCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                var headerBytesRead = await _dataReader.LoadAsync(messageHeaderSize, childCancellationTokenSource.Token);
                if (headerBytesRead > 0)
                {
                    var messageBodySize = _dataReader.ReadByte();
                    var messageType = _dataReader.ReadByte();

                    var bodyBytesReaded = await _dataReader.LoadAsync(messageBodySize, childCancellationTokenSource.Token);
                    if (bodyBytesReaded > 0)
                    {
                        foreach(var handler in _messageHandlers)
                        {
                            if(handler.CanDeserialize(messageType, messageBodySize))
                            {
                                //TODO DNF
                                //var message = handler.Deserialize(dataReaderObject, messageBodySize);
                                //await _messageBroker.Publish("SerialService", message);

                                //_logService.Info($"Recived UART message handled by {handler.GetType().Name}, Message details: [{message.ToString()}]");
                            }
                        }
                    }
                }
            }
        }

    }
}
