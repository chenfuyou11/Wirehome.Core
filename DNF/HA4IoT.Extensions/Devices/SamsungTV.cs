using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace HA4IoT.Extensions.Devices.Samsung
{
    public class SamsungTV
    {
        private readonly string _ip;
        private readonly int _port;
        private readonly string _appKey;
        private readonly string _nullValue = char.ToString((char)0x00);
        private const string AppString = "samsung.remote";

        public SamsungTV(string ip, int port, string appKey)
        {
            _ip = ip;
            _port = port;
            _appKey = appKey;
        }

        public void Send(string button)
        {
            var identifier = CreateIdentifier();
            var secondParameter = CreateSecondParameter();
            var command = CreateCommand(button);

            var messages = new List<byte[]>
                {
                    identifier,
                    secondParameter,
                    command
                };

            using (var socket = new Socket(SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Connect(_ip, _port);

                foreach (var message in messages.ToList())
                {
                    socket.Send(message);
                }
            }
        }

        private byte[] CreateIdentifier()
        {
            var nameBase64 = Base64Encode(_appKey);

            var message = char.ToString((char)0x64) + _nullValue + Format(nameBase64.Length) + _nullValue + nameBase64;
            var wrappedMessage = _nullValue + Format(AppString.Length) + _nullValue + AppString + Format(message.Length) + _nullValue + message;

            return ConvertToBytes(wrappedMessage);
        }

        private byte[] CreateSecondParameter()
        {
            var message = ((char)0xc8) + ((char)0x00) + string.Empty;

            var wrappedMessage = _nullValue + Format(AppString.Length) + _nullValue + AppString + Format(message.Length) + _nullValue + message;
            return ConvertToBytes(wrappedMessage);
        }

        private byte[] CreateCommand(string command)
        {
            var encodedCommand = Base64Encode(command);

            var message = _nullValue + _nullValue + _nullValue + char.ToString((char)encodedCommand.Length) + _nullValue + encodedCommand;
            var wrappedMessage = _nullValue + Format(AppString.Length) + _nullValue + AppString + Format(message.Length) + _nullValue + message;

            return ConvertToBytes(wrappedMessage);
        }

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        private static byte[] ConvertToBytes(string value)
        {
            return Encoding.ASCII.GetBytes(value);
        }

        private static string Format(int value)
        {
            return char.ToString((char)value);
        }
    }
}