using Wirehome.Actuators.Lamps;
using Wirehome.Contracts.Api;
using Wirehome.Contracts.Components.States;
using Wirehome.Tests.Mockups;
using Wirehome.Tests.Mockups.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Wirehome.Tests.Api
{
    [TestClass]
    public class ComponentApiTests
    {
        [TestMethod]
        public void API_InvokeCommand()
        {
            var testController = new TestController();
            var socket = new Lamp("Test", new TestLampAdapter());
            socket.ResetState();
            testController.AddComponent(socket);

            Assert.IsTrue(socket.GetState().Has(PowerState.Off));

            var parameter = new JObject
            {
                ["ComponentId"] = "Test",
                ["CommandType"] = "TurnOnCommand"
            };

            var apiCall = testController.InvokeApi("Service/IComponentRegistryService/ExecuteCommand", parameter);

            Assert.AreEqual(ApiResultCode.Success, apiCall.ResultCode);
            Assert.IsTrue(socket.GetState().Has(PowerState.On));
        }

        [TestMethod]
        public void API_InvokeUnknownCommand()
        {
            var testController = new TestController();

            var parameter = new JObject
            {
                ["ComponentId"] = "Test",
                ["CommandType"] = "TurnOnCommandXXX"
            };

            var apiCall = testController.InvokeApi("Service/IComponentRegistryService/ExecuteCommand", parameter);
            Assert.AreEqual(ApiResultCode.InvalidParameter, apiCall.ResultCode);
        }
    }
}
