using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Wirehome.ComponentModel.Messaging;
using Wirehome.Core.EventAggregator;
using Wirehome.Core.Native;
using Wirehome.Core.Services.Logging;

namespace Wirehome.Core.Services.Uart
{
    public class SerialMessagingService : ISerialMessagingService
    {
        private readonly CancellationTokenSource _readCancellationTokenSource = new CancellationTokenSource();
        private IBinaryReader _dataReader;

        private readonly ILogger _logService;
        private readonly INativeSerialDevice _serialDevice;
        private readonly List<IBinaryMessage> _messageHandlers = new List<IBinaryMessage>();
        private readonly IEventAggregator _eventAggregator;
        private readonly DisposeContainer _disposeContainer = new DisposeContainer();

        public SerialMessagingService(INativeSerialDevice serialDevice, ILogService logService,
            IEventAggregator eventAggregator, IEnumerable<IBinaryMessage> handlers)
        {
            _logService = logService.CreatePublisher(nameof(SerialMessagingService));
            _serialDevice = serialDevice ?? throw new ArgumentNullException(nameof(serialDevice));

            _messageHandlers.AddRange(handlers);
            _eventAggregator = eventAggregator;
        }

        public async Task Initialize()
        {
            await _serialDevice.Init();
            _dataReader = _serialDevice.GetBinaryReader();

            _disposeContainer.Add(_dataReader);
            _disposeContainer.Add(_serialDevice);

            Task.Run(async () => await Listen().ConfigureAwait(false));
        }

        public void Dispose() => _disposeContainer.Dispose();

        private async Task Listen()
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

        private void CloseDevice()
        {
            _serialDevice.Dispose();
        }

        public void CancelRead()
        {
            if (_readCancellationTokenSource != null && !_readCancellationTokenSource.IsCancellationRequested)
            {
                _readCancellationTokenSource.Cancel();
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
                        foreach (var handler in _messageHandlers)
                        {
                            if (handler.CanDeserialize(messageType, messageBodySize))
                            {
                                var message = handler.Deserialize(_dataReader, messageBodySize);
                                await _eventAggregator.Publish(message);

                                _logService.Info($"Received UART message handled by {handler.GetType().Name}, Message details: [{message}]");
                            }
                        }
                    }
                }
            }
        }
    }
}