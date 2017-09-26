using System;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Devices;
using Wirehome.Contracts.Hardware.I2C;
using Wirehome.Contracts.Scheduling;

namespace Wirehome.Hardware.Drivers.I2CHardwareBridge
{
    public class I2CHardwareBridge : IDevice
    {
        private readonly I2CSlaveAddress _address;
        private readonly II2CBusService _i2CBusService;

        public I2CHardwareBridge(string id, I2CSlaveAddress address, II2CBusService i2CBusService, ISchedulerService schedulerService)
        {
            if (schedulerService == null) throw new ArgumentNullException(nameof(schedulerService));
            _address = address;
            _i2CBusService = i2CBusService ?? throw new ArgumentNullException(nameof(i2CBusService));

            Id = id ?? throw new ArgumentNullException(nameof(id));

            //TODO
            //DHT22Accessor = new DHT22Accessor(this, schedulerService);
        }

        public string Id { get; }

        public DHT22Accessor DHT22Accessor { get; }

        public I2CSlaveAddress Address
        {
            get
            {
                return _address;
            }
        }

        public void ExecuteCommand(I2CHardwareBridgeCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            command.Execute(_address, _i2CBusService);
        }
    }
}
