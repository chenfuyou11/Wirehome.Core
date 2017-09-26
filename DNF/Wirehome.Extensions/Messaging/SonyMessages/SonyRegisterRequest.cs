using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Wirehome.Extensions.Messaging.SonyMessages
{
    public class SonyRegisterRequest : HttpMessage
    {
        public string ClientID { get; set; }
        public string ApplicationID { get; set; }
        public string PIN { get; set; }

        public SonyRegisterRequest()
        {
            RequestType = "POST";
        }

        public override string MessageAddress()
        {
            if (!string.IsNullOrWhiteSpace(PIN))
            {
                AuthorisationHeader =  new KeyValuePair<string, string>("Basic", Convert.ToBase64String(new UTF8Encoding().GetBytes(":" + PIN)));
            }
            return $"http://{Address}/sony/accessControl";
        }

        public override string Serialize()
        {
            return JsonConvert.SerializeObject(new
            {
                @method = "actRegister",
                @params = new object[] { new ActRegisterRequest(ClientID, ApplicationID, "private"), new[] { new ActRegister1Request("WOL", "yes") } },
                @id = 1,
                @version = "1.0",
            });  
        }

        public void CheckResponse(string responseBody)
        {
            var responseData = (JObject)JsonConvert.DeserializeObject(responseBody);

            var error = responseData.GetValue("error");

            if (!string.IsNullOrWhiteSpace(PIN))
            {
                var authKey = Cookies
                               .GetCookies(new Uri($"http://{Address}/sony/"))
                               .OfType<Cookie>()
                               .First(x => x.Name == "auth")
                               .Value;

                Cookies = new CookieContainer();
                Cookies.Add(new Uri($"http://{Address}/sony/"), new Cookie("auth", authKey, "/sony", Address));
            }
            //TODO
            //if (error != null)
            //{
            //    throw new BraviaApiException((int)error[0], (string)error[1]);
            //}
            //if (ex.ErrorId == 401)
            //{
            //    return true;
            //}
            //else
            //{
            //    throw;
            //}

            //var results = responseData.GetValue("results");
            //if (results != null)
            //{
            //    return (TResponse)results.ToObject(typeof(TResponse));
            //}
            //else
            //{
            //    if (typeof(TResponse).GetTypeInfo().ImplementedInterfaces.Contains(typeof(ICompositeResponse)))
            //    {
            //        var obj = Activator.CreateInstance<TResponse>() as ICompositeResponse;
            //        obj.ReadFromJson((JArray)responseData.GetValue("result"));
            //        return (TResponse)obj;
            //    }
            //    else
            //    {
            //        return (TResponse)responseData.GetValue("result").First().ToObject(typeof(TResponse));
            //    }
            //}
        }

        public class ActRegisterRequest
        {
            [JsonProperty("clientid")]
            public System.String Clientid { get; set; }
            [JsonProperty("nickname")]
            public System.String Nickname { get; set; }
            [JsonProperty("level")]
            public System.String Level { get; set; }
            public ActRegisterRequest() { }
            public ActRegisterRequest(System.String @clientid, System.String @nickname, System.String @level)
            {
                this.Clientid = @clientid;
                this.Nickname = @nickname;
                this.Level = @level;
            }
        }

        public class ActRegister1Request
        {
            [JsonProperty("function")]
            public System.String Function { get; set; }
            [JsonProperty("value")]
            public System.String Value { get; set; }
            public ActRegister1Request() { }
            public ActRegister1Request(System.String @function, System.String @value)
            {
                this.Function = @function;
                this.Value = @value;
            }
        }
    }
}
