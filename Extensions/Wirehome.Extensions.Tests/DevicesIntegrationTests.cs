using Wirehome.Configuration;
using Wirehome.Contracts.Components.Commands;
using Wirehome.Contracts.Components.Features;
using Wirehome.Contracts.Components.States;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Logging;
using Wirehome.Core;
using Wirehome.Extensions.Devices;
using Wirehome.Extensions.Extensions;
using Wirehome.Extensions.Messaging.Core;
using Wirehome.Extensions.Messaging.DenonMessages;
using Wirehome.Extensions.Messaging.KodiMessages;
using Wirehome.Extensions.Messaging.Services;
using Wirehome.Extensions.Networking;
using Wirehome.Extensions.Quartz;
using Wirehome.Extensions.Tests.Helpers;
using Wirehome.Scripting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Quartz;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;


namespace Wirehome.Extensions.Tests
{
    [TestClass]
    [TestCategory("Integration")]
    public partial class DevicesIntegrationTests
    {
        public const string DENON_HOST = "192.168.0.101";

        private (IEventAggregator ev, IScheduler ch) PrepareMocks()
        {
            var container = new Container(new ControllerOptions());
            container.RegisterSingleton<IContainer>(() => container);
            container.RegisterSingleton<IEventAggregator, EventAggregator>();
            container.RegisterQuartz();

            var log = Mock.Of<ILogService>();
            var scheduler = container.GetInstance<IScheduler>();
            var eventAggregator = container.GetInstance<IEventAggregator>();

            var http = new HttpMessagingService(log, eventAggregator);
            http.Startup();
            
            return (eventAggregator, scheduler);
        }

        [TestMethod]
        public async Task DenonTurnOn()
        {
            var mocks = PrepareMocks();

            var denon = new DenonAmplifier(Guid.NewGuid().ToString(), DENON_HOST, mocks.ev, mocks.ch);

            await denon.ExecuteAsyncCommand<TurnOnCommand>();
        }

        [TestMethod]
        public async Task DenonTurnOff()
        {
            var mocks = PrepareMocks();

            var denon = new DenonAmplifier(Guid.NewGuid().ToString(), DENON_HOST, mocks.ev, mocks.ch);

            await denon.ExecuteAsyncCommand<TurnOffCommand>();
        }

        [TestMethod]
        public async Task DenonStatus()
        {
            var tcs = TaskHelper.GenerateTimeoutTaskSource();
            var mocks = PrepareMocks();

            var denon = new DenonAmplifier(Guid.NewGuid().ToString(), DENON_HOST, mocks.ev, mocks.ch);
            await denon.Initialize();
            
            mocks.ev.Subscribe<DenonStatus>(x => { tcs.SetResult(true); });

            Assert.AreEqual(true, await tcs.Task);
        }

        

        //[TestMethod]
        //public async Task DenonControlMessage()
        //{
        //    var log = Mock.Of<ILogService>(); 
        //    var conf = new ConfigurationService(log);
        //    var script = new ScriptingService(conf, log);

        //    var brokre = new EventAggregator();

        //    var message = new DenonControlMessage
        //    {
        //        Command = "PowerOn",
        //        Api = "formiPhoneAppPower",
        //        ReturnNode = "Power",
        //        Address = "192.168.0.101"
        //    };

        //    var http = new HttpMessagingService(log, brokre);
        //    http.Startup();

        //    var test = new Stopwatch();
        //    test.Start();
            
        //    var result = await brokre.PublishWithResultAsync<DenonControlMessage, string>(message, millisecondsTimeOut: 999999);

        //    test.Stop();
        //}

        //[TestMethod]
        //public void WakeOnLanTest()
        //{
        //    WakeUpOnLan.Wakeup("9C-5C-8E-C2-01-62");
        //}

        //[TestMethod]
        //public async Task TestKod()
        //{
        //    //http://kodi.wiki/view/JSON-RPC_API/Examples#Introspect
        //    //http://kodi.wiki/view/JSON-RPC_API

        //    var ip = "192.168.0.159";
        //    var port = 8080;
        //    var uri = $"http://{ip}:{port}/jsonrpc";
        //    Dictionary<string, string> DefaultHeaders  = new Dictionary<string, string>();
        //    KeyValuePair<string, string> AuthorisationHeader  = new KeyValuePair<string, string>("", "");

        //   // DefaultHeaders.Add("Content-Type", "application/json-rpc");
        //    AuthorisationHeader = new KeyValuePair<string, string>("", "");

        //    //var jsonRpcRequest = new JsonRpcRequest
        //    //{
        //    //    Method = "JSONRPC.Ping",
        //    //    Parameters = new object()
        //    //};

        //    var jsonRpcRequest = new JsonRpcRequest
        //    {
        //        Method = "Player.PlayPause",
        //        Parameters = new { playerid = 1 }
        //    };

        //    //var jsonRpcRequest = new JsonRpcRequest
        //    //{
        //    //    Id = "1",
        //    //    Method = "Player.GetActivePlayers",
        //    //    Parameters = new object()
        //    //};


        //    var httpClientHandler = new HttpClientHandler();
        //    httpClientHandler.Credentials = new NetworkCredential("kodi", "9dominik");


        //    using (var httpClient = new HttpClient(httpClientHandler))
        //    {
        //        foreach (var header in DefaultHeaders)
        //        {
        //            httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
        //        }

        //        if (!string.IsNullOrWhiteSpace(AuthorisationHeader.Key))
        //        {
        //            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AuthorisationHeader.Key, AuthorisationHeader.Value);
        //        }

        //        var content = new StringContent(jsonRpcRequest.ToString());
        //        content.Headers.ContentType = new MediaTypeHeaderValue("application/json-rpc");

        //        var response = await httpClient.PostAsync(uri, content).ConfigureAwait(false);
        //        var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        //        var result = JsonConvert.DeserializeObject<JsonRpcResponse<JsonPausePlayResult>>(responseBody);
                
        //    }
        //}
    

        //[TestMethod]
        //public async Task SendRequest()
        //{
        //    //TURN ON
        //    //http://192.168.0.101/goform/formiPhoneAppPower.xml?1+PowerOn 
        //    //TURN OFF
        //    //http://192.168.0.101/goform/formiPhoneAppPower.xml?1+PowerStandby
        //    //string command = "PowerOn";
        //    //string api = "formiPhoneAppPower";
        //    //string returnNode = "Power";

        //    //SET VOLUME
        //    //http://192.168.0.101/goform/formiPhoneAppVolume.xml?1+-1.0 
        //    //string command = "-80.0";  // -80 do 0
        //    //string api = "formiPhoneAppVolume";
        //    //string returnNode = "MasterVolume";

        //    //http://192.168.0.101/goform/formiPhoneAppDirect.xml?MVUP
        //    //http://192.168.0.101/goform/formiPhoneAppDirect.xml?MVDOWN
        //    //No return

        //    //MUTE
        //    //http://192.168.0.101/goform/formiPhoneAppMute.xml?1+MuteOn
        //    //string command = "MuteOn"; 
        //    //string api = "formiPhoneAppMute";
        //    //string returnNode = "Mute";

        //    //CHANGE INPUT
        //    //http://192.168.0.101/goform/formiPhoneAppDirect.xml?SISAT/CBL 
        //    //No return


        //    //CHANGE CHANNEL
        //    //http://192.168.0.101/goform/formiPhoneAppTuner.xml?1+PRESETUP 

        //    //CHANGE PLAYBACK(FastForward, Next, Pause, Play, Previous, Rewind, StartOver, Stop)
            
        //    // Surround Mode
        //    // http://192.168.0.101/goform/formiPhoneAppDirect.xml?MSMCH%20STEREO 
        //    // http://192.168.0.101/goform/formiPhoneAppDirect.xml?MSDIRECT 

        //    //"http://192.168.0.101/goform/formMainZone_MainZoneXmlStatus.xml";
        //    //var inputs = xml.Descendants("InputFuncList").Descendants("value").Select(x => x.Value.Trim());
        //    //var renemaned = xml.Descendants("RenameSource").Descendants("value").Descendants("value").Select(x => x.Value.Trim());
        //    //var activeInput = xml.Descendants("InputFuncSelect").FirstOrDefault()?.Value?.Trim();
        //    //var powerStatus = xml.Descendants("Power").FirstOrDefault()?.Value?.Trim();
        //    //var surroundMode = xml.Descendants("SurrMode").FirstOrDefault()?.Value?.Trim();
        //    //var masterVolume = xml.Descendants("MasterVolume").FirstOrDefault()?.Value?.Trim();
        //    //var mute = xml.Descendants("Mute").FirstOrDefault()?.Value?.Trim();
        //    //var model = xml.Descendants("Model").FirstOrDefault()?.Value?.Trim();
            
        //    //http://192.168.0.101/goform/formMainZone_MainZoneXml.xml
        //    //var friendlyName = xml.Descendants("FriendlyName").FirstOrDefault()?.Value?.Trim();
        //    //var inputsMap = xml.Descendants("VideoSelectLists").Descendants("value").Select(x =>
        //    //new {
        //    //    Name = x.Attribute("index").Value,
        //    //    Value = x.Attribute("index").Value
        //    //});

        //    //http://192.168.0.101/goform/formMainZone_MainZoneXmlStatusLite.xml
        //    //http://192.168.0.101/goform/formZone2_Zone2XmlStatusLite.xml
        //    //var activeInput = xml.Descendants("InputFuncSelect").FirstOrDefault()?.Value?.Trim();
        //    //var powerStatus = xml.Descendants("Power").FirstOrDefault()?.Value?.Trim();
        //    //var masterVolume = xml.Descendants("MasterVolume").FirstOrDefault()?.Value?.Trim();
        //    //var mute = xml.Descendants("Mute").FirstOrDefault()?.Value?.Trim();


        //    string host = "192.168.0.101";
        //    string zone = "1";
        //    string command = "MuteOn";
        //    string api = "formiPhoneAppMute";
        //    //string returnNode = "Mute";

        //    var address = $"http://{host}/goform/{api}.xml?{zone}+{command}";
        //    address = "http://192.168.0.101/goform/formMainZone_MainZoneXml.xml";

        //    using (var httpClient = new HttpClient())
        //    {
        //        var httpResponse = await httpClient.GetAsync(address);
        //        httpResponse.EnsureSuccessStatusCode();
        //        using (var stream = await httpResponse.Content.ReadAsStreamAsync())
        //        {
        //            var xml = XDocument.Load(stream);
        //            //var test = xml.Descendants(returnNode).FirstOrDefault();
        //            //var value = test.Value;

        //            var friendlyName = xml.Descendants("FriendlyName").FirstOrDefault()?.Value?.Trim();
        //            var inputsMap = xml.Descendants("VideoSelectLists").Descendants("value").Select(x=>
        //            new {
        //               Name = x.Attribute("index").Value,
        //               Value = x.Attribute("index").Value
        //            }); 
        //        }
        //    }
            
        //    Assert.AreEqual(1, 1);
        //}

        //[TestMethod]
        //public void SendSamsung()
        //{ 
        //    //var remote = new SamsungTV("192.168.0.104", 55000, "Ha4Iot");

        //    //remote.Send("KEY_VOLDOWN");

        //    //KEY_POWEROFF
        //    //KEY_CHDOWN
        //    //KEY_CH_LIST
        //    //KEY_CHUP
        //    //KEY_SOURCE
        //    //KEY_CAPTION #Subt
        //    //KEY_GUIDE
        //    //KEY_INFO
        //    //KEY_MENU
        //    //KEY_PICTURE_SIZE
        //    //KEY_CONTENTS
        //    //KEY_TOOLS
        //    //KEY_AD
        //    //KEY_MTS #Dual
        //    //KEY_POWEROFF
        //    //KEY_RSS #Internet
        //    //KEY_W_LINK #Media P
        //    //KEY_ENTER
        //    //KEY_EXIT
        //    //KEY_DOWN
        //    //KEY_LEFT
        //    //KEY_RIGHT
        //    //KEY_UP
        //    //KEY_RETURN
        //    //KEY_0
        //    //KEY_1
        //    //KEY_2
        //    //KEY_FF
        //    //KEY_PAUSE
        //    //KEY_PLAY
        //    //KEY_REC
        //    //KEY_REWIND
        //    //KEY_STOP
        //    //KEY_MUTE
        //    //KEY_VOLDOWN
        //    //KEY_VOLUP

        //    Assert.AreEqual(1, 1);
        //}

        //[TestMethod]
        //public void SendSony()
        //{
        //    //var sony = new SonyBraviaTV("192.168.0.107", "8d76d476-4e75-4891-8888-22ffe33a3ef8", "Ha4IoT", 
        //    //    "13d2af6b9430422c41980548d64dd6cdf61e9b5e20325a9bbba29894b666c9fe");

        //    //sony.SendIrccAsync(RemoteControllerKeys.WakeUp);
        //}

        //public static class RemoteControllerKeys
        //{
        //    public const string Num1 = "AAAAAQAAAAEAAAAAAw==";
        //    public const string Num2 = "AAAAAQAAAAEAAAABAw==";
        //    public const string Num3 = "AAAAAQAAAAEAAAACAw==";
        //    public const string Num4 = "AAAAAQAAAAEAAAADAw==";
        //    public const string Num5 = "AAAAAQAAAAEAAAAEAw==";
        //    public const string Num6 = "AAAAAQAAAAEAAAAFAw==";
        //    public const string Num7 = "AAAAAQAAAAEAAAAGAw==";
        //    public const string Num8 = "AAAAAQAAAAEAAAAHAw==";
        //    public const string Num9 = "AAAAAQAAAAEAAAAIAw==";
        //    public const string Num0 = "AAAAAQAAAAEAAAAJAw==";
        //    public const string Num11 = "AAAAAQAAAAEAAAAKAw==";
        //    public const string Num12 = "AAAAAQAAAAEAAAALAw==";
        //    public const string Enter = "AAAAAQAAAAEAAAALAw==";
        //    public const string GGuide = "AAAAAQAAAAEAAAAOAw==";
        //    public const string ChannelUp = "AAAAAQAAAAEAAAAQAw==";
        //    public const string ChannelDown = "AAAAAQAAAAEAAAARAw==";
        //    public const string VolumeUp = "AAAAAQAAAAEAAAASAw==";
        //    public const string VolumeDown = "AAAAAQAAAAEAAAATAw==";
        //    public const string Mute = "AAAAAQAAAAEAAAAUAw==";
        //    public const string TvPower = "AAAAAQAAAAEAAAAVAw==";
        //    public const string Audio = "AAAAAQAAAAEAAAAXAw==";
        //    public const string MediaAudioTrack = "AAAAAQAAAAEAAAAXAw==";
        //    public const string Tv = "AAAAAQAAAAEAAAAkAw==";
        //    public const string Input = "AAAAAQAAAAEAAAAlAw==";
        //    public const string TvInput = "AAAAAQAAAAEAAAAlAw==";
        //    public const string TvAntennaCable = "AAAAAQAAAAEAAAAqAw==";
        //    public const string WakeUp = "AAAAAQAAAAEAAAAuAw==";
        //    public const string PowerOff = "AAAAAQAAAAEAAAAvAw==";
        //    public const string Sleep = "AAAAAQAAAAEAAAAvAw==";
        //    public const string Right = "AAAAAQAAAAEAAAAzAw==";
        //    public const string Left = "AAAAAQAAAAEAAAA0Aw==";
        //    public const string SleepTimer = "AAAAAQAAAAEAAAA2Aw==";
        //    public const string Analog2 = "AAAAAQAAAAEAAAA4Aw==";
        //    public const string TvAnalog = "AAAAAQAAAAEAAAA4Aw==";
        //    public const string Display = "AAAAAQAAAAEAAAA6Aw==";
        //    public const string Jump = "AAAAAQAAAAEAAAA7Aw==";
        //    public const string PicOff = "AAAAAQAAAAEAAAA+Aw==";
        //    public const string PictureOff = "AAAAAQAAAAEAAAA+Aw==";
        //    public const string Teletext = "AAAAAQAAAAEAAAA/Aw==";
        //    public const string Video1 = "AAAAAQAAAAEAAABAAw==";
        //    public const string Video2 = "AAAAAQAAAAEAAABBAw==";
        //    public const string AnalogRgb1 = "AAAAAQAAAAEAAABDAw==";
        //    public const string Home = "AAAAAQAAAAEAAABgAw==";
        //    public const string Exit = "AAAAAQAAAAEAAABjAw==";
        //    public const string PictureMode = "AAAAAQAAAAEAAABkAw==";
        //    public const string Confirm = "AAAAAQAAAAEAAABlAw==";
        //    public const string Up = "AAAAAQAAAAEAAAB0Aw==";
        //    public const string Down = "AAAAAQAAAAEAAAB1Aw==";
        //    public const string ClosedCaption = "AAAAAgAAAKQAAAAQAw==";
        //    public const string Component1 = "AAAAAgAAAKQAAAA2Aw==";
        //    public const string Component2 = "AAAAAgAAAKQAAAA3Aw==";
        //    public const string Wide = "AAAAAgAAAKQAAAA9Aw==";
        //    public const string EPG = "AAAAAgAAAKQAAABbAw==";
        //    public const string PAP = "AAAAAgAAAKQAAAB3Aw==";
        //    public const string TenKey = "AAAAAgAAAJcAAAAMAw==";
        //    public const string BSCS = "AAAAAgAAAJcAAAAQAw==";
        //    public const string Ddata = "AAAAAgAAAJcAAAAVAw==";
        //    public const string Stop = "AAAAAgAAAJcAAAAYAw==";
        //    public const string Pause = "AAAAAgAAAJcAAAAZAw==";
        //    public const string Play = "AAAAAgAAAJcAAAAaAw==";
        //    public const string Rewind = "AAAAAgAAAJcAAAAbAw==";
        //    public const string Forward = "AAAAAgAAAJcAAAAcAw==";
        //    public const string DOT = "AAAAAgAAAJcAAAAdAw==";
        //    public const string Rec = "AAAAAgAAAJcAAAAgAw==";
        //    public const string Return = "AAAAAgAAAJcAAAAjAw==";
        //    public const string Blue = "AAAAAgAAAJcAAAAkAw==";
        //    public const string Red = "AAAAAgAAAJcAAAAlAw==";
        //    public const string Green = "AAAAAgAAAJcAAAAmAw==";
        //    public const string Yellow = "AAAAAgAAAJcAAAAnAw==";
        //    public const string SubTitle = "AAAAAgAAAJcAAAAoAw==";
        //    public const string CS = "AAAAAgAAAJcAAAArAw==";
        //    public const string BS = "AAAAAgAAAJcAAAAsAw==";
        //    public const string Digital = "AAAAAgAAAJcAAAAyAw==";
        //    public const string Options = "AAAAAgAAAJcAAAA2Aw==";
        //    public const string Media = "AAAAAgAAAJcAAAA4Aw==";
        //    public const string Prev = "AAAAAgAAAJcAAAA8Aw==";
        //    public const string Next = "AAAAAgAAAJcAAAA9Aw==";
        //    public const string DpadCenter = "AAAAAgAAAJcAAABKAw==";
        //    public const string CursorUp = "AAAAAgAAAJcAAABPAw==";
        //    public const string CursorDown = "AAAAAgAAAJcAAABQAw==";
        //    public const string CursorLeft = "AAAAAgAAAJcAAABNAw==";
        //    public const string CursorRight = "AAAAAgAAAJcAAABOAw==";
        //    public const string ShopRemoteControlForcedDynamic = "AAAAAgAAAJcAAABqAw==";
        //    public const string FlashPlus = "AAAAAgAAAJcAAAB4Aw==";
        //    public const string FlashMinus = "AAAAAgAAAJcAAAB5Aw==";
        //    public const string AudioQualityMode = "AAAAAgAAAJcAAAB7Aw==";
        //    public const string DemoMode = "AAAAAgAAAJcAAAB8Aw==";
        //    public const string Analog = "AAAAAgAAAHcAAAANAw==";
        //    public const string Mode3D = "AAAAAgAAAHcAAABNAw==";
        //    public const string DigitalToggle = "AAAAAgAAAHcAAABSAw==";
        //    public const string DemoSurround = "AAAAAgAAAHcAAAB7Aw==";
        //    public const string StarAD = "AAAAAgAAABoAAAA7Aw==";
        //    public const string AudioMixUp = "AAAAAgAAABoAAAA8Aw==";
        //    public const string AudioMixDown = "AAAAAgAAABoAAAA9Aw==";
        //    public const string PhotoFrame = "AAAAAgAAABoAAABVAw==";
        //    public const string Tv_Radio = "AAAAAgAAABoAAABXAw==";
        //    public const string SyncMenu = "AAAAAgAAABoAAABYAw==";
        //    public const string Hdmi1 = "AAAAAgAAABoAAABaAw==";
        //    public const string Hdmi2 = "AAAAAgAAABoAAABbAw==";
        //    public const string Hdmi3 = "AAAAAgAAABoAAABcAw==";
        //    public const string Hdmi4 = "AAAAAgAAABoAAABdAw==";
        //    public const string TopMenu = "AAAAAgAAABoAAABgAw==";
        //    public const string PopUpMenu = "AAAAAgAAABoAAABhAw==";
        //    public const string OneTouchTimeRec = "AAAAAgAAABoAAABkAw==";
        //    public const string OneTouchView = "AAAAAgAAABoAAABlAw==";
        //    public const string DUX = "AAAAAgAAABoAAABzAw==";
        //    public const string FootballMode = "AAAAAgAAABoAAAB2Aw==";
        //    public const string iManual = "AAAAAgAAABoAAAB7Aw==";
        //    public const string Netflix = "AAAAAgAAABoAAAB8Aw==";
        //    public const string Assists = "AAAAAgAAAMQAAAA7Aw==";
        //    public const string ActionMenu = "AAAAAgAAAMQAAABLAw==";
        //    public const string Help = "AAAAAgAAAMQAAABNAw==";
        //    public const string TvSatellite = "AAAAAgAAAMQAAABOAw==";
        //    public const string WirelessSubwoofer = "AAAAAgAAAMQAAAB+Aw==";
        //}
    }
}
