using System.Linq;
using System.Xml.Linq;
using System.IO;
using HA4IoT.Extensions.Messaging.Core;

namespace HA4IoT.Extensions.Messaging.DenonMessages
{
    public class DenonControlMessage : HttpMessage
    {
        public string Command { get; set; }
        public string Api { get; set; }
        public string ReturnNode { get; set; }
        public string Zone { get; set; } = "1";

        public DenonControlMessage()
        {
            RequestType = "GET";
        }

        public override string MessageAddress()
        {
            return $"http://{Address}/goform/{Api}.xml?{Zone}+{Command}";
        }

        public override object ParseResult(string responseData)
        {
            using (var reader = new StringReader(responseData))
            {
                var xml = XDocument.Load(reader);
                var returnNode = xml.Descendants(ReturnNode).FirstOrDefault();
                return returnNode.Value;
            }
        }
    }
}