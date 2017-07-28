
using HA4IoT.Contracts.Logging;
using HA4IoT.Contracts.Messaging;
using HA4IoT.Extensions.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;

namespace HA4IoT.Extensions
{
    public class SerialService : ISerialService
    {
        private SerialDevice serialPort = null;
        private CancellationTokenSource ReadCancellationTokenSource;
        private DataReader dataReaderObject = null;
        private readonly ILogger _logService;
        private readonly IMessageBrokerService _messageBroker;
        private readonly List<IMessageHandler> _messageHandlers = new List<IMessageHandler>();

        public SerialService(ILogService logService, IMessageBrokerService messageBroker, IEnumerable<IMessageHandler> handlers)
        {
            _logService = logService.CreatePublisher(nameof(SerialService));
            _messageBroker = messageBroker;
            _messageHandlers.AddRange(handlers);
        }

        public async void Startup()
        {
            var devices = await DeviceInformation.FindAllAsync(SerialDevice.GetDeviceSelector());
            var firstDevice = devices.FirstOrDefault();

            serialPort = await SerialDevice.FromIdAsync(firstDevice.Id);
            if (serialPort == null) throw new NotFoundException("UART port not found on device");

            // Configure serial settings
            serialPort.WriteTimeout = TimeSpan.FromMilliseconds(1000);
            serialPort.ReadTimeout = TimeSpan.FromMilliseconds(1000);
            serialPort.BaudRate = 115200;
            serialPort.Parity = SerialParity.None;
            serialPort.StopBits = SerialStopBitCount.One;
            serialPort.DataBits = 8;
            serialPort.Handshake = SerialHandshake.None;

            //Create cancellation token object to close I/ O operations when closing the device
            ReadCancellationTokenSource = new CancellationTokenSource();

            Listen();
        }

        private async void Listen()
        {
            try
            {
                if (serialPort != null)
                {
                    dataReaderObject = new DataReader(serialPort.InputStream);
                    dataReaderObject.ByteOrder = ByteOrder.LittleEndian;

                    while (true)
                    {
                        await ReadAsync(ReadCancellationTokenSource.Token);
                    }
                }
            }
            catch (TaskCanceledException tce)
            {
                CloseDevice();
            }
            catch (Exception ex)
            {
                _logService.Error(ex.ToString());
            }
            finally
            {
                // Cleanup once complete
                if (dataReaderObject != null)
                {
                    dataReaderObject.DetachStream();
                    dataReaderObject = null;
                }
            }
        }

        public void RegisterHandler(IMessageHandler handler)
        {
            _messageHandlers.Add(handler);
        }

        public void Close()
        {
            CancelReadTask();
            CloseDevice();  
        }

        private void CloseDevice()
        {
            if (serialPort != null)
            {
                serialPort.Dispose();
            }
            serialPort = null;
        }

        private void CancelReadTask()
        {
            if (ReadCancellationTokenSource != null)
            {
                if (!ReadCancellationTokenSource.IsCancellationRequested)
                {
                    ReadCancellationTokenSource.Cancel();
                }
            }
        }

        private async Task ReadAsync(CancellationToken cancellationToken)
        {
            const uint messageHeaderSize = 2;
            cancellationToken.ThrowIfCancellationRequested();
            dataReaderObject.InputStreamOptions = InputStreamOptions.Partial;

            using (var childCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                var headerBytesRead = await dataReaderObject.LoadAsync(messageHeaderSize).AsTask(childCancellationTokenSource.Token);
                if (headerBytesRead > 0)
                {
                    var messageBodySize = dataReaderObject.ReadByte();
                    var messageType = dataReaderObject.ReadByte();

                    var bodyBytesReaded = await dataReaderObject.LoadAsync(messageBodySize).AsTask(childCancellationTokenSource.Token);
                    if (bodyBytesReaded > 0)
                    {
                        foreach(var handler in _messageHandlers)
                        {
                            if(handler.CanHandleUart(messageType, messageBodySize))
                            {
                                var message = handler.ReadUart(dataReaderObject, messageBodySize);
                                await _messageBroker.Publish("SerialService", message);

                                _logService.Info($"Recived UART message handled by {handler.GetType().Name}, Message details: [{message.ToString()}]");
                            }
                        }
                    }
                }
            }
        }

    }
}
