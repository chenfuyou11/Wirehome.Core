using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;
using Wirehome.ComponentModel.Adapters.Denon;
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

            var adapter = configuration.config.Adapters.FirstOrDefault(x => x.Uid == "Denon");

            await adapter.Initialize();
        }
    }
}