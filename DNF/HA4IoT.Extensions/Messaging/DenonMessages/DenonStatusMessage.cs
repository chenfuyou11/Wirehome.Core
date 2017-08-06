using System.Linq;
using System.Xml.Linq;
using System.IO;

namespace HA4IoT.Extensions.Messaging.DenonMessages
{
    public class DenonStatusMessage : HttpMessage
    {
        public DenonStatusMessage()
        {
            RequestType = "GET";
        }

        public override string GetAddress()
        {
            return $"http://{Address}/goform/formMainZone_MainZoneXmlStatus.xml";
        }

        public override void ValidateResponse(string responseBody)
        {
            using (var reader = new StringReader(responseBody))
            {
                var xml = XDocument.Load(responseBody);
                var inputs = xml.Descendants("InputFuncList").Descendants("value").Select(x => x.Value.Trim());
                var renemaned = xml.Descendants("RenameSource").Descendants("value").Descendants("value").Select(x => x.Value.Trim());
                var activeInput = xml.Descendants("InputFuncSelect").FirstOrDefault()?.Value?.Trim();
                var powerStatus = xml.Descendants("Power").FirstOrDefault()?.Value?.Trim();
                var surroundMode = xml.Descendants("SurrMode").FirstOrDefault()?.Value?.Trim();
                var masterVolume = xml.Descendants("MasterVolume").FirstOrDefault()?.Value?.Trim();
                var mute = xml.Descendants("Mute").FirstOrDefault()?.Value?.Trim();
                var model = xml.Descendants("Model").FirstOrDefault()?.Value?.Trim();
            }
        } 
    }
}