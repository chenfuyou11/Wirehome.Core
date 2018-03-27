using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Quartz;
using Quartz.Spi;
using SimpleInjector;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Wirehome.ComponentModel.Adapters;
using Wirehome.ComponentModel.Commands;
using Wirehome.ComponentModel.Configuration;
using Wirehome.Core.Communication.I2C;
using Wirehome.Core.EventAggregator;
using Wirehome.Core.Services.DependencyInjection;
using Wirehome.Core.Services.Logging;
using Wirehome.Core.Services.Quartz;
using Wirehome.ComponentModel.Extensions;
using Wirehome.ComponentModel.Events;
using Wirehome.ComponentModel.Capabilities;
using Wirehome.ComponentModel.ValueTypes;
using System.Collections.Generic;
using Wirehome.Core.Tests.ComponentModel;

namespace Wirehome.Extensions.Tests
{
    [TestClass]
    public class AdaptersTests : ReactiveTest
    {
        [TestMethod]
        public async Task CreateAdapter()
        {
            var configuration = await CommonIntegrationcs.ReadConfiguration("componentConiguration");

            var adapter = configuration.config.Adapters.FirstOrDefault(x => x.Uid == "HSRel8_1");

            await adapter.Initialize();
            await adapter.ExecuteCommand(Command.RefreshCommand);
        }
    }
}