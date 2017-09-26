using Wirehome.Conditions;
using Wirehome.Contracts.Conditions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wirehome.Tests.Actuators
{
    [TestClass]
    public class ConditionsValidatorTests
    {
        [TestMethod]
        public void ShouldBeNotFulfilled_WithNotFulfilledDefault_AndOneFulfilledCondiiton()
        {
            var conditionsValidator = new ConditionsValidator()
                .WithCondition(ConditionRelation.Or, new NotFulfilledTestCondition())
                .WithCondition(ConditionRelation.Or, new FulfilledTestCondition())
                .WithDefaultState(ConditionState.NotFulfilled);

            Assert.AreEqual(ConditionState.Fulfilled, conditionsValidator.Validate());
        }
    }
}
