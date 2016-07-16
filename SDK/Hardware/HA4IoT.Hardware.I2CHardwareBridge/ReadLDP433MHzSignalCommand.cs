using System;
using HA4IoT.Contracts.Hardware;
using System.Collections.Generic;

namespace HA4IoT.Hardware.I2CHardwareBridge
{
    public class ReadLDP433MHzSignalCommand : I2CHardwareBridgeCommand
    {
        private const byte I2C_ACTION = 4;
        private const byte ACTION_REGISTER_SENSOR = 0;
        private const int READ_COMMAND_LENGTH = 5;
        private byte _pin;

        public ReadLDP433MHzSignalCommandResponse Response { get; private set; }

        public ReadLDP433MHzSignalCommand WithPin(byte pin)
        {
            _pin = pin;
            return this;
        }

        public override void Execute(II2CDevice i2CDevice)
        {
            i2CDevice.Write(GenerateRegisterSensorPackage());
            var codes = new List<uint>();
            int remaining = 0;

            do
            {
                byte[] buffer = new byte[READ_COMMAND_LENGTH];
                i2CDevice.Read(buffer);

                uint code = BitConverter.ToUInt32(buffer, 0);
                remaining = buffer[4];

                if (code != 0)
                {
                    codes.Add(code);
                }

            }
            while (remaining > 0);

            Response = new ReadLDP433MHzSignalCommandResponse(codes);
        }

        private byte[] GenerateRegisterSensorPackage()
        {
            return new[]
            {
                I2C_ACTION,
                ACTION_REGISTER_SENSOR,
                _pin
            };
        }
    }
}
