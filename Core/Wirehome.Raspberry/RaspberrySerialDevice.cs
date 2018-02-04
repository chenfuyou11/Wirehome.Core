using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Wirehome.Contracts.Core;

namespace Wirehome.Raspberry
{
    public class RaspberrySerialDevice : INativeSerialDevice
    {
        private SerialDevice _serialDevice;

        public void Dispose()
        {
            if (_serialDevice != null)
            {
                _serialDevice.Dispose();
            }
            _serialDevice = null;
        }

        public async Task Init()
        {
            var devices = await DeviceInformation.FindAllAsync(SerialDevice.GetDeviceSelector());
            var firstDevice = devices.FirstOrDefault();

            _serialDevice = await SerialDevice.FromIdAsync(firstDevice.Id);
            if (_serialDevice == null) throw new Exception("UART port not found on device");

            _serialDevice.WriteTimeout = TimeSpan.FromMilliseconds(1000);
            _serialDevice.ReadTimeout = TimeSpan.FromMilliseconds(1000);
            _serialDevice.BaudRate = 115200;
            _serialDevice.Parity = SerialParity.None;
            _serialDevice.StopBits = SerialStopBitCount.One;
            _serialDevice.DataBits = 8;
            _serialDevice.Handshake = SerialHandshake.None;
        }

        public IBinaryReader GetBinaryReader()
        {
            return new BinaryReader(_serialDevice.InputStream);
        }
    }
}
