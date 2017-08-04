using System;
using System.Text;
using Newtonsoft.Json.Linq;
using Windows.Storage.Streams;
using HA4IoT.Extensions.Contracts;

namespace HA4IoT.Extensions.Messaging
{
    public class DenonMessage : IMessage
    {
        public string ParamName { get; set; }
        public string ParamValue { get; set; }
        public string DeviceAddress { get; set; }
        
        public string MessageAddress(JObject message)
        {
            var denonMessage = message.ToObject<DenonMessage>();

            return "http://" + denonMessage?.DeviceAddress + "/MainZone/index.put.asp";
        }

        public bool CanDeserialize(byte messageType, byte messageSize)
        {
            return false;
        }

        public bool CanSerialize(string messageType)
        {
            return messageType == GetType().Name;
        }

        public object Deserialize(IDataReader reader, byte? messageSize = null)
        {
            throw new NotImplementedException();
        }

        public byte[] Serialize(JObject message)
        {
            var denonMessage = message.ToObject<DenonMessage>();

            string parameterString = "";
            parameterString += denonMessage?.ParamName + "=" + denonMessage?.ParamValue + "&";
            parameterString = parameterString.Substring(0, parameterString.Length - 1);

            return Encoding.UTF8.GetBytes(parameterString);
        }

        public MessageType Type()
        {
            return MessageType.Denon;
        }
    }
}
