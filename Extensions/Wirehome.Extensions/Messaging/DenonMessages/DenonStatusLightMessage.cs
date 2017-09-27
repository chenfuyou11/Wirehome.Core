using System.Linq;
using System.Xml.Linq;
using System.IO;
using Wirehome.Extensions.Devices;

namespace Wirehome.Extensions.Messaging.DenonMessages
{
    public class DenonStatusLightMessage : HttpMessage
    {
        public string Zone { get; set; } = "1";

        public DenonStatusLightMessage()
        {
            RequestType = "GET";
        }

        public override string MessageAddress()
        {
            if(Zone == "1")
            {
                return $"http://{Address}/goform/formMainZone_MainZoneXmlStatusLite.xml";
            }
            return $"http://{Address}/goform/formZone{Zone}_Zone2XmlStatusLite.xml";
        }

        public override object ParseResult(string responseBody)
        {
            using (var reader = new StringReader(responseBody))
            {
                var xml = XDocument.Parse(responseBody);

                return new DenonStatus
                {
                    ActiveInput = xml.Descendants("InputFuncSelect").FirstOrDefault()?.Value?.Trim(),
                    PowerStatus = xml.Descendants("Power").FirstOrDefault()?.Value?.Trim(),
                    MasterVolume = xml.Descendants("MasterVolume").FirstOrDefault()?.Value?.Trim(),
                    Mute = xml.Descendants("Mute").FirstOrDefault()?.Value?.Trim()
                };
            }
        } 
    }
}