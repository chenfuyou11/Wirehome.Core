using Wirehome.Contracts.Components.Commands;
using Wirehome.Contracts.Components.States;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Logging;
using Wirehome.Extensions.Devices;
using Wirehome.Extensions.Extensions;
using Wirehome.Extensions.Messaging.Core;
using Wirehome.Extensions.Messaging.Services;
using Wirehome.Extensions.Quartz;
using Wirehome.Extensions.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Quartz;
using System;
using System.Threading.Tasks;
using Wirehome.Extensions.Devices.Commands;
using Wirehome.Core;
using Wirehome.Extensions.Devices.Denon;

namespace Wirehome.Extensions.Tests
{
    [TestClass]
    [TestCategory("Integration")]
    public class DenonTests
    {
        public const string DENON_HOST = "192.168.0.101";

        private (IEventAggregator ev, IScheduler ch) PrepareMocks()
        {
            var log = Mock.Of<ILogService>();

            var container = new Container(new ControllerOptions());
            container.RegisterSingleton<IContainer>(() => container);
            container.RegisterSingleton<IEventAggregator, EventAggregator>();
            container.RegisterSingleton(log);
            container.RegisterSingleton<DenonStateJob>();
            container.RegisterQuartz();
            
            var scheduler = container.GetInstance<IScheduler>();
            var eventAggregator = container.GetInstance<IEventAggregator>();

            var http = new HttpMessagingService(eventAggregator);
            http.Initialize();
            
            return (eventAggregator, scheduler);
        }

        [TestMethod]
        public async Task DenonPowerTest()
        {
            var mocks = PrepareMocks();

            var denon = new DenonAmplifier(Guid.NewGuid().ToString(), mocks.ev, mocks.ch)
            {
                StatusInterval = TimeSpan.FromMilliseconds(500),
                Hostname = DENON_HOST
            };

            await denon.Initialize();

            await denon.ExecuteAsyncCommand<TurnOffCommand>();

            denon.TryGetPowerState(out PowerStateValue pstate);

            await Task.Delay(1000);

            await denon.ExecuteAsyncCommand<TurnOnCommand>();

            denon.TryGetPowerState(out PowerStateValue pstate2);

            Assert.AreEqual(PowerStateValue.Off, pstate);
            Assert.AreEqual(PowerStateValue.On, pstate2);
        }
        
        [TestMethod]
        public async Task DenonVolumeTest()
        {
            var mocks = PrepareMocks();

            var denon = new DenonAmplifier(Guid.NewGuid().ToString(), mocks.ev, mocks.ch)
            {
                StatusInterval = TimeSpan.FromMilliseconds(500),
                Hostname = DENON_HOST
            };
            await denon.Initialize();

            await denon.ExecuteAsyncCommand(new SetVolumeCommand { Volume = 20 });
            
            await denon.ExecuteAsyncCommand(new VolumeUpCommand { DefaultChangeFactor = 10 });

            var tcs = TaskHelper.GenerateTimeoutTaskSource<float?>(2000);
            mocks.ev.Subscribe<DenonStatus>(x => tcs.SetResult(x.Message.MasterVolume));
            var result = await tcs.Task;

            Assert.AreEqual(20 + 10, result);
        }

        [TestMethod]
        public async Task DenonMuteTest()
        {
            var mocks = PrepareMocks();

            var denon = new DenonAmplifier(Guid.NewGuid().ToString(), mocks.ev, mocks.ch)
            {
                StatusInterval = TimeSpan.FromMilliseconds(500),
                Hostname = DENON_HOST
            };
            await denon.Initialize();

            await denon.ExecuteAsyncCommand<MuteOnCommand>();

            var tcs = TaskHelper.GenerateTimeoutTaskSource<bool>(2000);
            mocks.ev.Subscribe<DenonStatus>(x => tcs.SetResult(x.Message.Mute));
            var result = await tcs.Task;

            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public async Task DenonInputSourceTest()
        {
            var mocks = PrepareMocks();

            var denon = new DenonAmplifier(Guid.NewGuid().ToString(), mocks.ev, mocks.ch)
            {
                StatusInterval = TimeSpan.FromMilliseconds(500),
                Hostname = DENON_HOST
            };
            await denon.Initialize();

            await denon.ExecuteAsyncCommand(new ChangeInputSourceCommand("DVD"));

            await Task.Delay(1000);

            var tcs = TaskHelper.GenerateTimeoutTaskSource<string>(2000);
            mocks.ev.Subscribe<DenonStatus>(x => tcs.SetResult(x.Message.ActiveInput));
            var result = await tcs.Task;

            Assert.AreEqual("DVD", result);
        }

        [TestMethod]
        public async Task DenonSurroundTest()
        {
            var mocks = PrepareMocks();

            var denon = new DenonAmplifier(Guid.NewGuid().ToString(), mocks.ev, mocks.ch)
            {
                StatusInterval = TimeSpan.FromMilliseconds(500),
                Hostname = DENON_HOST
            };
            await denon.Initialize();

            await denon.ExecuteAsyncCommand(new ChangeSurroundModeCommand("Music"));

            await Task.Delay(1000);

            await denon.RefreshDeviceState();

            denon.TryGetSurroundState(out string surroundState);

            Assert.AreEqual(DenonSurroundModes.GetCommandResult("Music"), surroundState);
        }

        //Tuner
        //http://192.168.0.101/goform/formTuner_TunerXml.xml 
        // Tune init
        //http://192.168.0.101/Tuner/TUNER/index.html.init.asp?ZoneName=ZONE2 
        // Tuner FM
        //http://192.168.0.101/Tuner/TUNER/index.put.asp?cmd0=PutTunerBand%2FAM&ZoneName=ZONE2 

        // Tuner set channel
        //http://192.168.0.101/Tuner/TUNER/index.put.asp?cmd0=PutTunerPreset%2F04&cmd1=aspMainZone_WebUpdateStatus%2F
        //http://192.168.0.101/Tuner/TUNER/index.put.asp?cmd0=PutTunerPreset%2F05&cmd1=aspMainZone_WebUpdateStatus%2F

        // Tune up
        //http://192.168.0.101/goform/formiPhoneAppDirect.xml?TFANUP 

        // Tune mode manual/auto
        //http://192.168.0.101/goform/formiPhoneAppDirect.xml?TMANMANUAL 
        //http://192.168.0.101/goform/formiPhoneAppDirect.xml?TMANAUTO 


    }
}
