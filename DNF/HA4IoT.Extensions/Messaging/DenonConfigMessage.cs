using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Windows.Storage.Streams;
using HA4IoT.Extensions.Contracts;

namespace HA4IoT.Extensions.Messaging
{
    public class DenonConfigMessage : IMessage
    {
        public string DeviceAddress { get; set; }
        
        public string MessageAddress(JObject message)
        {
            var denonMessage = message.ToObject<DenonMessage>();
            return  "http://" + denonMessage?.DeviceAddress + "/goform/formMainZone_MainZoneXml.xml";
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
            //var denonMessage = message.ToObject<DenonMessage>();

            //HttpClient httpClient = new HttpClient();
            //XmlDocument xml = new XmlDocument();

            //var httpResponse = await httpClient.GetAsync(_denonConfigAddress);
            //httpResponse.EnsureSuccessStatusCode();
            //using (var stream = await httpResponse.Content.ReadAsStreamAsync())
            //{
            //    xml.Load(stream);
            //}

            //return xml;
            return new byte[] { };
        }

        public MessageType Type()
        {
            return MessageType.Denon;
        }
    }
}
