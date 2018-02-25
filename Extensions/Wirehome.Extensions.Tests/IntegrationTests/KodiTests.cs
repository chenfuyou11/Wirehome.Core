//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Moq;
//using Quartz;
//using System;
//using System.Threading.Tasks;
//using Wirehome.Contracts.Components.Commands;
//using Wirehome.Contracts.Core;
//using Wirehome.Contracts.Logging;
//using Wirehome.Core;
//using Wirehome.Core.EventAggregator;
//using Wirehome.Extensions.Devices;
//using Wirehome.Extensions.Devices.Commands;
//using Wirehome.Extensions.Devices.Kodi;
//using Wirehome.Extensions.Messaging.Core;
//using Wirehome.Extensions.Messaging.Services;
//using Wirehome.Extensions.Quartz;

//namespace Wirehome.Extensions.Tests
//{
//    [TestClass]
//    [TestCategory("Integration")]
//    public class KodiTests
//    {
//        public const string KODI_HOST = "192.168.0.159";
//        public const string KODI_USER = "kodi";
//        public const string KODI_PASS = "kajak";
//        public const int KODI_PORT = 8080;

//        private (IEventAggregator ev, IScheduler ch) PrepareMocks()
//        {
//            var log = Mock.Of<ILogService>();

//            var container = new Container(new ControllerOptions());
//            container.RegisterSingleton<IContainer>(() => container);
//            container.RegisterSingleton<IEventAggregator, EventAggregator>();
//            container.RegisterSingleton(log);
//            container.RegisterSingleton<DenonStateJob>();
//            container.RegisterQuartz();

//            var scheduler = container.GetInstance<IScheduler>();
//            var eventAggregator = container.GetInstance<IEventAggregator>();

//            var http = new HttpMessagingService(eventAggregator);
//            http.Initialize();

//            return (eventAggregator, scheduler);
//        }


//        [TestMethod]
//        public async Task KodiPowerTest()
//        {
//            var mocks = PrepareMocks();

//            var denon = new KodiDevice(Guid.NewGuid().ToString(), mocks.ev, mocks.ch)
//            {
//                StatusInterval = TimeSpan.FromMilliseconds(500),
//                Hostname = KODI_HOST,
//                UserName = KODI_USER,
//                Password = KODI_PASS,
//                Port= KODI_PORT
//            };

//            await denon.Initialize().ConfigureAwait(false);

//            await denon.ExecuteAsyncCommand<TurnOnCommand>().ConfigureAwait(false);

//            await Task.Delay(2000).ConfigureAwait(false);

//            await denon.ExecuteAsyncCommand<TurnOffCommand>().ConfigureAwait(false);
//        }

//        [TestMethod]
//        public async Task KodiVolumeTest()
//        {
//            var mocks = PrepareMocks();

//            var denon = new KodiDevice(Guid.NewGuid().ToString(), mocks.ev, mocks.ch)
//            {
//                StatusInterval = TimeSpan.FromMilliseconds(500),
//                Hostname = KODI_HOST,
//                UserName = KODI_USER,
//                Password = KODI_PASS,
//                Port = KODI_PORT
//            };

//            await denon.Initialize().ConfigureAwait(false);

//            //await denon.ExecuteAsyncCommand(new SetVolumeCommand { Volume = 50 }).ConfigureAwait(false);
//            await denon.ExecuteAsyncCommand(new MuteOnCommand()).ConfigureAwait(false);
//        }

//        [TestMethod]
//        public async Task KodiPlayerTest()
//        {
//            var mocks = PrepareMocks();

//            var denon = new KodiDevice(Guid.NewGuid().ToString(), mocks.ev, mocks.ch)
//            {
//                StatusInterval = TimeSpan.FromMilliseconds(500),
//                Hostname = KODI_HOST,
//                UserName = KODI_USER,
//                Password = KODI_PASS,
//                Port = KODI_PORT,
//                PlayerId = 1
//            };

//            await denon.Initialize().ConfigureAwait(false);
//            await denon.ExecuteAsyncCommand(new PlayCommand()).ConfigureAwait(false);            
//        }

//    }
//}

////{"jsonrpc": "2.0", "method": "Application.GetProperties", "id": "libMovies", "params": { "properties": ["volume", "muted", "name", "version"] }}

////{"jsonrpc": "2.0", "method": "Player.GetActivePlayers", "id": 1}
////{"id": "KodiJSON-RPC", "jsonrpc": "2.0", "method":"Player.Open","params":{"item":{"movieid":3 }} }

////{"jsonrpc": "2.0", "method": "Player.GetItem", "params": { "properties": ["title", "album", "artist", "duration", "thumbnail", "file", "fanart", "streamdetails"], "playerid": 1 }, "id": "AudioGetItem"}
////{"jsonrpc": "2.0", "method": "Player.GetItem", "params": { "properties": ["title"], "playerid": 1 }, "id": "AudioGetItem"}
////{"jsonrpc": "2.0", "method": "Player.PlayPause", "params": { "playerid": 1 }, "id": 1}
////{"jsonrpc": "2.0", "method": "Player.GetProperties", "id": "libMovies", "params": { "properties": ["speed", "subtitles", "subtitleenabled", "position"], "playerid": 1 }}
////{"jsonrpc": "2.0", "method": "Player.Stop", "id": "libMovies", "params": {  "playerid": 1 }}
////{"jsonrpc": "2.0", "method": "Player.Seek", "id": "libMovies", "params": {  "playerid": 1,"value":{ "hours":0, "minutes":10, "seconds":0} }}


////{"jsonrpc": "2.0", "method": "VideoLibrary.GetMovies", "id": "libMovies"}

////{"jsonrpc": "2.0", "method": "VideoLibrary.GetMovies", "params": { "filter": {"field": "playcount", "operator": "is", "value": "0"}, "limits": { "start" : 0, "end": 75 }, "properties" : ["art", "rating", "thumbnail", "playcount", "file"], "sort": { "order": "ascending", "method": "label", "ignorearticle": true } }, "id": "libMovies"}


////{"jsonrpc": "2.0", "method": "VideoLibrary.Scan", "id": "AudioGetItem"}


////{"jsonrpc": "2.0", "method": "GUI.ActivateWindow", "id": "libMovies", "params": {  "window": "videos" }}
////{"jsonrpc": "2.0", "method": "GUI.ActivateWindow", "id": "libMovies", "params": {  "window": "subtitlesearch" }}
////{"jsonrpc": "2.0", "method": "GUI.ActivateWindow", "id": "libMovies", "params": {  "window": "home" }}

//// Player.SetSubtitle
//// Input.Select
//// Input.Up

////PlaybackController 

////Pause
////Play
////Stop

////Next
////Previous
////StartOver

////FastForward
////Rewind