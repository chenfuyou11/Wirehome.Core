using System;
using System.Net;

namespace Wirehome.Extensions.Messaging.SonyMessages
{
    class SonyControlMessage : HttpMessage
    {
        public string Code { get; set; }
        public string AuthorisationKey { get; set; }

        public SonyControlMessage()
        {
            RequestType = "POST";
            DefaultHeaders.Add("SOAPACTION", "\"urn:schemas-sony-com:service:IRCC:1#X_SendIRCC\"");
        }

        public override string MessageAddress()
        {
            Cookies.Add(new Uri($"http://{Address}/sony/"), new Cookie("auth", AuthorisationKey, "/sony", Address));
            return $"http://{Address}/sony/IRCC";
        }

        public override string Serialize()
        {
            return $@"<?xml version=""1.0""?>
                    <s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"" s:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/"">
                        <s:Body>
                        <u:X_SendIRCC xmlns:u=""urn:schemas-sony-com:service:IRCC:1"">
                            <IRCCCode>{Code}</IRCCCode>
                        </u:X_SendIRCC>
                        </s:Body>
                    </s:Envelope>";
        }
    }
}
