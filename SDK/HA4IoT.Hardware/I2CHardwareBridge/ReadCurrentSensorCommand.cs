using System;
using HA4IoT.Contracts.Hardware;

namespace HA4IoT.Hardware.I2CHardwareBridge
{
    public class ReadCurrentSensorCommand : I2CHardwareBridgeCommand
    {
        private const byte I2C_ACTION = 5;
        private const byte ACTION_REGISTER_SENSOR = 0;
        private const byte I2C_PACKAGE_SIZE = 5;

        private byte _pin;

        public ReadCurrentSensorCommandResponse Response { get; private set; }

        public ReadCurrentSensorCommand WithPin(byte pin)
        {
            _pin = pin;
            return this;
        }

        public override void Execute(II2CDevice i2CDevice)
        {
            i2CDevice.Write(GenerateRegisterSensorPackage());

            byte[] buffer = new byte[I2C_PACKAGE_SIZE];
            i2CDevice.Read(buffer);

            ParseResponse(buffer);
        }

        private void ParseResponse(byte[] buffer)
        {
            float current = 0.0F;

            bool succeeded = buffer[0] == 1;

            if (succeeded)
            {
                current = BitConverter.ToSingle(buffer, 1);
            }

            Response = new ReadCurrentSensorCommandResponse(succeeded, current);
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
