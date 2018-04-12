using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wirehome.ComponentModel.Adapters;
using Wirehome.ComponentModel.Capabilities;
using Wirehome.ComponentModel.Commands;
using Wirehome.ComponentModel.Events;
using Wirehome.ComponentModel.Extensions;
using Wirehome.ComponentModel.ValueTypes;
using Wirehome.Core.EventAggregator;
using Wirehome.Core.Tests.ComponentModel;

namespace Wirehome.Extensions.Tests
{
    [TestClass]
    public class ComponentIntegrationTests : ReactiveTest
    {
        [TestMethod]
        public async Task TestReadComponentsFromConfig()
        {
            var config = await CommonIntegrationcs.ReadConfiguration("componentConiguration");
            var eventAggregator = config.container.GetInstance<IEventAggregator>();

            var properyChangeEvent = new PropertyChangedEvent("HSPE16InputOnly_1", PowerState.StateName, new BooleanValue(false),
                                     new BooleanValue(true), new Dictionary<string, IValue>() { { AdapterProperties.PinNumber, new IntValue(2) } });

            await eventAggregator.PublishDeviceEvent(properyChangeEvent, new string[] { AdapterProperties.PinNumber });

            //var lamp = configuration.Components.FirstOrDefault(c => c.Uid == "Lamp1");
            //await lamp.ExecuteCommand(new Command { Type = CommandType.TurnOn }).ConfigureAwait(false);

            //await Task.Delay(5000);
        }


        [TestMethod]
        public async Task TestRemoteLamp()
        {
            var config = await CommonIntegrationcs.ReadConfiguration("componentConiguration");
            var eventAggregator = config.container.GetInstance<IEventAggregator>();
            
            var lamp = config.config.Components.FirstOrDefault(c => c.Uid == "RemoteLamp");
            await lamp.ExecuteCommand(Command.TurnOnCommand).ConfigureAwait(false);

            
        }

    }
}