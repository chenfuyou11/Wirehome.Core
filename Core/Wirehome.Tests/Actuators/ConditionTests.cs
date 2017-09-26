using Wirehome.Actuators.Lamps;
using Wirehome.Components;
using Wirehome.Conditions;
using Wirehome.Conditions.Specialized;
using Wirehome.Contracts.Components.States;
using Wirehome.Contracts.Conditions;
using Wirehome.Tests.Mockups.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wirehome.Tests.Actuators
{
    [TestClass]
    public class ConditionTests
    {
        [TestMethod]
        public void ConditionWithTrueExpression_ShouldBeFulfilled()
        {
            var condition = new Condition().WithExpression(() => true);
            Assert.AreEqual(condition.Validate(), ConditionState.Fulfilled);
        }

        [TestMethod]
        public void ConditionWithFulfilledExpression_ShouldBeFulfilled()
        {
            var condition = new Condition().WithExpression(() => ConditionState.Fulfilled);
            Assert.AreEqual(condition.Validate(), ConditionState.Fulfilled);
        }

        [TestMethod]
        public void ConditionWithFalseExpression_ShouldBeNotFulfilled()
        {
            var condition = new Condition().WithExpression(() => false);
            Assert.AreEqual(condition.Validate(), ConditionState.NotFulfilled);
        }

        [TestMethod]
        public void ConditionWithNotfulfilledExpression_ShouldBeNotFulfilled()
        {
            var condition = new Condition().WithExpression(() => ConditionState.NotFulfilled);
            Assert.AreEqual(condition.Validate(), ConditionState.NotFulfilled);
        }

        [TestMethod]
        public void FulfilledCondition_AND_FulfilledCondition_ShouldBeFulfilled()
        {
            var condition = new FulfilledTestCondition().WithRelatedCondition(ConditionRelation.And, new FulfilledTestCondition());
            Assert.AreEqual(condition.Validate(), ConditionState.Fulfilled);
        }

        [TestMethod]
        public void FulfilledCondition_AND_NotFulfilledCondition_ShouldBeNotFulfilled()
        {
            var condition = new FulfilledTestCondition().WithRelatedCondition(ConditionRelation.And,  new NotFulfilledTestCondition());
            Assert.AreEqual(condition.Validate(), ConditionState.NotFulfilled);
        }

        [TestMethod]
        public void NotFulfilledCondition_AND_FulfilledCondition_ShouldBeNotFulfilled()
        {
            var condition = new NotFulfilledTestCondition().WithRelatedCondition(ConditionRelation.And, new FulfilledTestCondition());
            Assert.AreEqual(condition.Validate(), ConditionState.NotFulfilled);
        }

        [TestMethod]
        public void NotFulfilledCondition_OR_FulfilledCondition_ShouldBeFulfilled()
        {
            var condition = new NotFulfilledTestCondition().WithRelatedCondition(ConditionRelation.Or, new FulfilledTestCondition());
            Assert.AreEqual(condition.Validate(), ConditionState.Fulfilled);
        }

        [TestMethod]
        public void FulfilledCondition_OR_NotFulfilledCondition_ShouldBeFulfilled()
        {
            var condition = new FulfilledTestCondition().WithRelatedCondition(ConditionRelation.Or, new NotFulfilledTestCondition());
            Assert.AreEqual(condition.Validate(), ConditionState.Fulfilled);
        }

        [TestMethod]
        public void FulfilledCondition_ANDNOT_NotFulfilledCondition_ShouldBeFulfilled()
        {
            var condition = new FulfilledTestCondition().WithRelatedCondition(ConditionRelation.AndNot, new NotFulfilledTestCondition());
            Assert.AreEqual(condition.Validate(), ConditionState.Fulfilled);
        }
        
        [TestMethod]
        public void FulfilledCondition_ANDNOT_NotFulfilledCondition_ShouldBeNotFulfilled()
        {
            var condition = new FulfilledTestCondition().WithRelatedCondition(ConditionRelation.AndNot, new FulfilledTestCondition());
            Assert.AreEqual(condition.Validate(), ConditionState.NotFulfilled);
        }

        [TestMethod]
        public void FulfilledCondition_ORNOT_FulfilledCondition_ShouldBeNotFulfilled()
        {
            var condition = new FulfilledTestCondition().WithRelatedCondition(ConditionRelation.OrNot, new FulfilledTestCondition());
            Assert.AreEqual(condition.Validate(), ConditionState.Fulfilled);
        }

        [TestMethod]
        public void FulfilledCondition_ORNOT_NotFulfilledCondition_ShouldBeNotFulfilled()
        {
            var condition = new FulfilledTestCondition().WithRelatedCondition(ConditionRelation.OrNot, new NotFulfilledTestCondition());
            Assert.AreEqual(condition.Validate(), ConditionState.Fulfilled);
        }

        [TestMethod]
        public void NotFulfilledCondition_ORNOT_FulfilledCondition_ShouldBeNotFulfilled()
        {
            var condition = new NotFulfilledTestCondition().WithRelatedCondition(ConditionRelation.OrNot, new FulfilledTestCondition());
            Assert.AreEqual(condition.Validate(), ConditionState.NotFulfilled);
        }

        [TestMethod]
        public void ConditionWithMultipleRelatedConditionsMustBeFulfilled()
        {
            var condition = new FulfilledTestCondition().WithRelatedCondition(ConditionRelation.And, new FulfilledTestCondition()).WithRelatedCondition(ConditionRelation.Or, new NotFulfilledTestCondition());
            Assert.AreEqual(condition.Validate(), ConditionState.Fulfilled);
        }

        [TestMethod]
        public void ConditionWithMultipleRelatedConditionsMustBeNotFulfilled()
        {
            var condition = new FulfilledTestCondition().WithRelatedCondition(ConditionRelation.And, new FulfilledTestCondition()).WithRelatedCondition(ConditionRelation.And, new NotFulfilledTestCondition());
            Assert.AreEqual(condition.Validate(), ConditionState.NotFulfilled);
        }

        [TestMethod]
        public void ComponentHasStateCondition()
        {
            var output = new Lamp("Test", new TestLampAdapter());
            output.TryReset();

            var condition = new ComponentHasStateCondition(output, PowerState.Off);
            Assert.IsTrue(condition.IsFulfilled());

            output.TryTurnOn();

            Assert.IsFalse(condition.IsFulfilled());
        }
    }
}
