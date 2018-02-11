using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.IO;
using System.Threading.Tasks;
using Wirehome.Api;
using Wirehome.Api.Configuration;
using Wirehome.Contracts.Api;
using Wirehome.Contracts.Areas;
using Wirehome.Contracts.Components;
using Wirehome.Contracts.Configuration;
using Wirehome.Contracts.Logging;
using Wirehome.Contracts.Settings;

namespace Wirehome.Extensions.Tests
{
    [TestClass]
    public class AlexaSkillTests
    {
        [TestMethod]
        public void DiscoverRequest()
        {
            var testMessage = ReadMessage("Discover.Request");

            var service = new Alexa.Service.Function();
            service.FunctionHandler(testMessage, null);

            //Assert.AreEqual(true, lampDictionary[LivingroomId].GetIsTurnedOn());
        }

        [TestMethod]
        public void PowerOnRequest()
        {
            var testMessage = ReadMessage("Power.On.Request");

            var service = new Alexa.Service.Function();
            service.FunctionHandler(testMessage, null);

            //Assert.AreEqual(true, lampDictionary[LivingroomId].GetIsTurnedOn());
        }

        [TestMethod]
        public async Task WebServerTest()
        {
            var logService = Mock.Of<ILogService>();
            var logger = Mock.Of<ILogger>();
            var apiDispatcherService = Mock.Of<IApiDispatcherService>();
            var configurationService = Mock.Of<IConfigurationService>();
            var areService = Mock.Of<IAreaRegistryService>();
            var settingService = Mock.Of<ISettingsService>();
            var componentService = Mock.Of<IComponentRegistryService>();
            var port = 81;
            
            Mock.Get(logService).Setup(x => x.CreatePublisher(It.IsAny<string>())).Returns(logger);
            Mock.Get(configurationService).Setup(x => x.GetConfiguration<HttpServerServiceConfiguration>(It.IsAny<string>())).Returns(new HttpServerServiceConfiguration
            {
                Port = port
            });

            var httpService = new HttpServerService(configurationService, apiDispatcherService, logService);
            await httpService.Initialize().ConfigureAwait(false);
            
            var alexaDispatcher = new AlexaDispatcherService(httpService, areService, settingService, componentService, logService);
            await alexaDispatcher.Initialize().ConfigureAwait(false);

            var testMessage = ReadMessage("Discover.Request");

            var service = new Alexa.Service.Function();
            service.HandlerUri = $"http://localhost:{port}/alexa"; ;
            await service.FunctionHandler(testMessage, null).ConfigureAwait(false);
        }


        private string ReadMessage(string message) => File.ReadAllText($@"Alexa\SampleMessages\{message}.json");
        
    }
}
