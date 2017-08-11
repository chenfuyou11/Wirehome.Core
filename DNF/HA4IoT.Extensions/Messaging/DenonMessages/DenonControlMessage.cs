using System.Linq;
using System.Xml.Linq;
using System.IO;

namespace HA4IoT.Extensions.Messaging.DenonMessages
{
    public class DenonControlMessage : HttpMessage
    {
        public string Command { get; set; }
        public string Api { get; set; }
        public string ReturnNode { get; set; }
        public string Zone { get; set; }

        public DenonControlMessage()
        {
            RequestType = "GET";
        }

        public override string MessageAddress()
        {
            return $"http://{Address}/goform/{Api}.xml?{Zone}+{Command}";
        }

        public override void ValidateResponse(string responseBody)
        {
            using (var reader = new StringReader(responseBody))
            {
                var xml = XDocument.Load(responseBody);
                var returnNode = xml.Descendants(ReturnNode).FirstOrDefault();
                var returnValue = returnNode.Value;
            }
        } 
    }
}