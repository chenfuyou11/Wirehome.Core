using Wirehome.Contracts.Areas;
using Wirehome.PersonalAgent;
using Wirehome.Tests.Mockups;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wirehome.Tests.PersonalAgent
{
    [TestClass]
    public class MessageContextTests
    {
        [TestMethod]
        public void IdentifyAreaIds()
        {
            var testController = new TestController();
            var areaRegistry = testController.GetInstance<IAreaRegistryService>();
            areaRegistry.RegisterArea("Test").Settings.Caption = "Büro";

            var messageContextFactory = testController.GetInstance<MessageContextFactory>();
            var messageContext = messageContextFactory.Create("Hello World. Büro.");
            Assert.AreEqual(1, messageContext.IdentifiedAreaIds.Count);
            Assert.AreEqual(0, messageContext.IdentifiedComponentIds.Count);
            Assert.AreEqual(0, messageContext.IdentifiedCommands.Count);
        }
    }
}
