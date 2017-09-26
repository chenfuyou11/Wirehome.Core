using System;
using System.Collections.Generic;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Hardware.I2C;
using Wirehome.Contracts.Logging;
using Wirehome.Contracts.Scripting;
using Wirehome.Contracts.Services;

namespace Wirehome.Hardware.I2C
{
    public sealed class I2CBusService : ServiceBase, II2CBusService
    {
        private readonly Dictionary<int, INativeI2cDevice> _deviceCache = new Dictionary<int, INativeI2cDevice>();
        private readonly string _busId;
        private readonly ILogger _log;
        private readonly INativeI2cDevice _nativeI2CDevice;

        public I2CBusService(ILogService logService, IScriptingService scriptingService, INativeI2cDevice nativeI2CDevice)
        {
            if (scriptingService == null) throw new ArgumentNullException(nameof(scriptingService));
            _log = logService?.CreatePublisher(nameof(I2CBusService)) ?? throw new ArgumentNullException(nameof(logService));
            _nativeI2CDevice = nativeI2CDevice ?? throw new ArgumentNullException(nameof(nativeI2CDevice));

            _busId = _nativeI2CDevice.GetBusId();

            scriptingService.RegisterScriptProxy(s => new I2CBusScriptProxy(this));
        }

        public II2CTransferResult Write(I2CSlaveAddress address, byte[] buffer, bool useCache = true)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));

            return Execute(address, d => d.WritePartial(buffer), useCache);
        }

        public II2CTransferResult Read(I2CSlaveAddress address, byte[] buffer, bool useCache = true)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));

            return Execute(address, d => d.ReadPartial(buffer), useCache);
        }

        public II2CTransferResult WriteRead(I2CSlaveAddress address, byte[] writeBuffer, byte[] readBuffer, bool useCache = true)
        {
            if (writeBuffer == null) throw new ArgumentNullException(nameof(writeBuffer));
            if (readBuffer == null) throw new ArgumentNullException(nameof(readBuffer));

            return Execute(address, d => d.WriteReadPartial(writeBuffer, readBuffer), useCache);
        }

        private II2CTransferResult Execute(I2CSlaveAddress address, Func<INativeI2cDevice, NativeI2cTransferResult> action, bool useCache = true)
        {
            lock (_deviceCache)
            {
                INativeI2cDevice device = null;
                try
                {
                    device = GetDevice(address.Value, useCache);
                    var result = action(device);
                    
                    if (result.Status != NativeI2cTransferStatus.FullTransfer)
                    {
                        _log.Warning($"Transfer failed. Address={address.Value} Status={result.Status} TransferredBytes={result.BytesTransferred}");
                    }

                    return WrapResult(result);
                }
                catch (Exception exception)
                {
                    // Ensure that the application will not crash if some devices are currently not available etc.
                    _log.Warning(exception, $"Error while accessing I2C device with address {address}.");
                    return new I2CTransferResult(I2CTransferStatus.UnknownError, 0);
                }
                finally
                {
                    if (!useCache)
                    {
                        device?.Dispose();
                    }
                }
            }
        }

        private static II2CTransferResult WrapResult(NativeI2cTransferResult result)
        {
            var status = I2CTransferStatus.UnknownError;
            switch (result.Status)
            {
                case NativeI2cTransferStatus.FullTransfer:
                    {
                        status = I2CTransferStatus.FullTransfer;
                        break;
                    }

                case NativeI2cTransferStatus.PartialTransfer:
                    {
                        status = I2CTransferStatus.PartialTransfer;
                        break;
                    }

                case NativeI2cTransferStatus.ClockStretchTimeout:
                    {
                        status = I2CTransferStatus.ClockStretchTimeout;
                        break;
                    }

                case NativeI2cTransferStatus.SlaveAddressNotAcknowledged:
                    {
                        status = I2CTransferStatus.SlaveAddressNotAcknowledged;
                        break;
                    }
            }

            return new I2CTransferResult(status, (int)result.BytesTransferred);
        }

        private INativeI2cDevice GetDevice(int address, bool useCache)
        {
            // The Arduino Nano T&H bridge does not work correctly when reusing the device. More investigation is required!
            // At this time, the cache can be disabled for certain devices.
            if (!useCache)
            {
                return CreateDevice(address);
            }

            if (!_deviceCache.TryGetValue(address, out INativeI2cDevice device))
            {
                device = CreateDevice(address);
                _deviceCache.Add(address, device);
            }

            return device;
        }

        private INativeI2cDevice CreateDevice(int slaveAddress)
        {
            return _nativeI2CDevice.CreateDevice(_busId, slaveAddress);
        }
        
    }
}