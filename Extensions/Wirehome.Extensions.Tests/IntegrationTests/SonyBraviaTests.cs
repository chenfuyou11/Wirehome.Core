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
//using Wirehome.Extensions.Devices.Commands;
//using Wirehome.Extensions.Devices.Sony;
//using Wirehome.Extensions.Messaging.Core;
//using Wirehome.Extensions.Messaging.Services;
//using Wirehome.Extensions.Quartz;

//namespace Wirehome.Extensions.Tests.IntegrationTests
//{
//    [TestClass]
//    [TestCategory("Integration")]
//    public class SonyBraviaTests
//    {
//        public const string SONY_HOST = "192.168.0.106";
//        public const string SONY_KEY = "344fb7d28691ba43c9045e9d38fa3c83a0587620fc14a79a958db4ff5d30e0e7";
//        //1b230496280590265a59311916db216c230d6a3d9a9008fa8ea91b49ec4b772e

//        private (IEventAggregator ev, IScheduler ch) PrepareMocks()
//        {
//            var log = Mock.Of<ILogService>();

//            var container = new Container(new ControllerOptions());
//            container.RegisterSingleton<IContainer>(() => container);
//            container.RegisterSingleton<IEventAggregator, EventAggregator>();
//            container.RegisterSingleton(log);
//            container.RegisterQuartz();

//            var scheduler = container.GetInstance<IScheduler>();
//            var eventAggregator = container.GetInstance<IEventAggregator>();

//            var http = new HttpMessagingService(eventAggregator);
//            http.Initialize();

//            return (eventAggregator, scheduler);
//        }

//        [TestMethod]
//        public async Task SonyPowerTest()
//        {
//            var mocks = PrepareMocks();

//            var sony = new SonyBraviaTV(Guid.NewGuid().ToString(), mocks.ev, mocks.ch)
//            {
//                Hostname = SONY_HOST,
//                AuthorisationKey = SONY_KEY
//            };
//            await sony.Initialize();

//            await sony.ExecuteAsyncCommand<TurnOnCommand>().ConfigureAwait(false);
//        }

//        [TestMethod]
//        public async Task SonyVolumeTest()
//        {
//            var mocks = PrepareMocks();

//            var sony = new SonyBraviaTV(Guid.NewGuid().ToString(), mocks.ev, mocks.ch)
//            {
//                Hostname = SONY_HOST,
//                AuthorisationKey = SONY_KEY
//            };
//            await sony.Initialize();

//            await sony.ExecuteAsyncCommand(new SetVolumeCommand { Volume = 40 }).ConfigureAwait(false);
//        }
//        //
//    }
//}


//    public const string ChannelUp = "AAAAAQAAAAEAAAAQAw==";
//    public const string ChannelDown = "AAAAAQAAAAEAAAARAw==";
//    public const string VolumeUp = "AAAAAQAAAAEAAAASAw==";
//    public const string VolumeDown = "AAAAAQAAAAEAAAATAw==";
//    public const string Mute = "AAAAAQAAAAEAAAAUAw==";
//    public const string TvPower = "AAAAAQAAAAEAAAAVAw==";
//    public const string Tv = "AAAAAQAAAAEAAAAkAw==";
//    public const string Input = "AAAAAQAAAAEAAAAlAw==";
//    public const string TvInput = "AAAAAQAAAAEAAAAlAw==";
//    public const string TvAntennaCable = "AAAAAQAAAAEAAAAqAw==";
//    public const string WakeUp =   "AAAAAQAAAAEAAAAuAw==";
//    public const string PowerOff = "AAAAAQAAAAEAAAAvAw==";
//    public const string Sleep = "AAAAAQAAAAEAAAAvAw==";
//    public const string Analog2 = "AAAAAQAAAAEAAAA4Aw==";
//    public const string TvAnalog = "AAAAAQAAAAEAAAA4Aw==";
//    public const string Display = "AAAAAQAAAAEAAAA6Aw==";
//    public const string Jump = "AAAAAQAAAAEAAAA7Aw==";
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
//    public const string Stop = "AAAAAgAAAJcAAAAYAw==";
//    public const string Pause = "AAAAAgAAAJcAAAAZAw==";
//    public const string Play = "AAAAAgAAAJcAAAAaAw==";
//    public const string Rewind = "AAAAAgAAAJcAAAAbAw==";
//    public const string Forward = "AAAAAgAAAJcAAAAcAw==";
//    public const string Return = "AAAAAgAAAJcAAAAjAw==";
//    public const string Digital = "AAAAAgAAAJcAAAAyAw==";
//    public const string Analog = "AAAAAgAAAHcAAAANAw==";
//    public const string Hdmi1 = "AAAAAgAAABoAAABaAw==";
//    public const string Hdmi2 = "AAAAAgAAABoAAABbAw==";
//    public const string Hdmi3 = "AAAAAgAAABoAAABcAw==";
//    public const string Hdmi4 = "AAAAAgAAABoAAABdAw==";
//    public const string Netflix = "AAAAAgAAABoAAAB8Aw==";
//    public const string TvSatellite = "AAAAAgAAAMQAAABOAw==";

