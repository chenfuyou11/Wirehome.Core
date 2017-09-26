using System;
using Wirehome.Contracts.Conditions;

namespace Wirehome.Conditions
{
    public class RelatedCondition
    {
        public ConditionRelation Relation { get; private set; } = ConditionRelation.And;

        public ICondition Condition { get; private set; }

        public RelatedCondition WithCondition(ICondition condition)
        {
            if (condition == null) throw new ArgumentNullException(nameof(condition));

            Condition = condition;
            return this;
        }

        public RelatedCondition WithRelation(ConditionRelation relation)
        {
            Relation = relation;
            return this;
        }

        public ConditionState Validate()
        {
            if (Condition == null)
            {
                throw new InvalidOperationException("Related condition has no condition.");
            }

            return Condition.Validate();
        }
    }
}
