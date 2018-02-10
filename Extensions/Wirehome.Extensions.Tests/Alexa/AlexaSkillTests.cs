using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

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

        private string ReadMessage(string message) => File.ReadAllText($@"Alexa\SampleMessages\{message}.json");
        
    }
}
