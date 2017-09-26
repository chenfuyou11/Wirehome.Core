using System.Linq;
using System.Xml.Linq;
using System.IO;

namespace Wirehome.Extensions.Messaging.DenonMessages
{
    public class DenonMappingMessage : HttpMessage
    {
        public DenonMappingMessage()
        {
            RequestType = "GET";
        }

        public override string MessageAddress()
        {
            return $"http://{Address}/goform/formMainZone_MainZoneXml.xml";
        }

        public override object ParseResult(string responseBody)
        {
            using (var reader = new StringReader(responseBody))
            {
                var xml = XDocument.Load(responseBody);
                var friendlyName = xml.Descendants("FriendlyName").FirstOrDefault()?.Value?.Trim();
                var inputsMap = xml.Descendants("VideoSelectLists").Descendants("value").Select(x =>
                new
                {
                    Name = x.Attribute("index").Value,
                    Value = x.Attribute("index").Value
                });

                //TODO
                return "";
            }
        } 
    }
}