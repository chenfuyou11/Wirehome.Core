using System.Linq;
using System.Xml.Linq;
using System.IO;

namespace HA4IoT.Extensions.Messaging.DenonMessages
{
    public class DenonStatusLightMessage : HttpMessage
    {
        public string Zone { get; set; }

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

        public override void ValidateResponse(string responseBody)
        {
            using (var reader = new StringReader(responseBody))
            {
                var xml = XDocument.Load(responseBody);
                var activeInput = xml.Descendants("InputFuncSelect").FirstOrDefault()?.Value?.Trim();
                var powerStatus = xml.Descendants("Power").FirstOrDefault()?.Value?.Trim();
                var masterVolume = xml.Descendants("MasterVolume").FirstOrDefault()?.Value?.Trim();
                var mute = xml.Descendants("Mute").FirstOrDefault()?.Value?.Trim();
            }
        } 
    }
}