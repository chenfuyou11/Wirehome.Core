using HA4IoT.Components;
using System;
using HA4IoT.Contracts.Components;
using HA4IoT.Contracts.Components.States;
using HA4IoT.Contracts.Components.Features;
using HA4IoT.Contracts.Components.Commands;
using HA4IoT.Components.Commands;
using System.Net;
using System.Text;
using System.IO;
using System.Xml;
using System.Net.Http;
using System.Threading.Tasks;

namespace HA4IoT.Extensions.Devices
{
    public class Denon : ComponentBase
    {
        private PowerStateValue _powerState = PowerStateValue.Off;
        private CommandExecutor _commandExecutor;
        private readonly string _denonControlAddress;
        private readonly string _denonConfigAddress;

        public Denon(string id, string hostname) : base(id)
        {
            _commandExecutor = new CommandExecutor();
            _commandExecutor.Register<TurnOnCommand>(c => 
            {
                SendCommand("cmd0", "PutZone_OnOff/ON");
            });
            _commandExecutor.Register<TurnOffCommand>(c =>
            {
                SendCommand("cmd0", "PutZone_OnOff/OFF");
            }
            );

            _denonControlAddress = "http://" + hostname + "/MainZone/index.put.asp";
            _denonConfigAddress = "http://" + hostname + "/goform/formMainZone_MainZoneXml.xml";
        }

        public override void ExecuteCommand(ICommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            _commandExecutor.Execute(command);
        }

        public override IComponentFeatureStateCollection GetState()
        {
            var state = new ComponentFeatureStateCollection()
                .With(new PowerState(_powerState));

            return state;
        }


        public override IComponentFeatureCollection GetFeatures()
        {
            var features = new ComponentFeatureCollection()
                .With(new PowerStateFeature());

            return features;
        }

        private void SetStateInternal(PowerStateValue powerState, bool forceUpdate = false)
        {

            if (!forceUpdate && _powerState == powerState)
            {
                return;
            }
            
            var oldState = GetState();
            
            _powerState = powerState;

            OnStateChanged(oldState);
        }

      
        public async void SendCommand(string paramName, string paramValue)
        {
            try
            {
                Uri uri = new Uri(_denonControlAddress);
                WebRequest webRequest = WebRequest.Create(uri);

                webRequest.Method = "POST";
                webRequest.ContentType = "application/x-www-form-urlencoded";

                string parameterString = "";
                parameterString += paramName + "=" + paramValue + "&";
                parameterString = parameterString.Substring(0, parameterString.Length - 1);

                byte[] byteArray = Encoding.UTF8.GetBytes(parameterString);
                webRequest.Headers["ContentLength"] = byteArray.Length.ToString();

                using (Stream stream = await webRequest.GetRequestStreamAsync())
                {
                    stream.Write(byteArray, 0, byteArray.Length);
                }

                using (WebResponse response = await webRequest.GetResponseAsync())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        using (StreamReader sr = new StreamReader(stream))
                        {
                            string responseStream = sr.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        private async Task<XmlDocument> GetStateAsync(string hostname)
        {
            HttpClient httpClient = new HttpClient();
            XmlDocument xml = new XmlDocument();
            
            var httpResponse = await httpClient.GetAsync(_denonConfigAddress);
            httpResponse.EnsureSuccessStatusCode();
            using (var stream = await httpResponse.Content.ReadAsStreamAsync())
            {
                xml.Load(stream);
            }

            return xml;
                    
        }
    }

    
}
