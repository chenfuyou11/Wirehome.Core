using System;
using System.Collections.Generic;
using System.Linq;
using Wirehome.ComponentModel;
using Wirehome.Contracts.Conditions;
using Wirehome.Core.Services.DependencyInjection;

namespace Wirehome.Conditions
{
    public class Condition : BaseObject
    {
        private readonly ConditionsValidator _relatedConditionsValidator;

        public Func<ConditionState> Expression { get; private set; } = () => ConditionState.Fulfilled;

        public IList<RelatedCondition> RelatedConditions { get; } = new List<RelatedCondition>();

        public Condition()
        {
            _relatedConditionsValidator = new ConditionsValidator(RelatedConditions);
        }

        public ConditionState Validate()
        {
            var thisState = Expression();

            if (IsInverted)
            {
                thisState = thisState == ConditionState.Fulfilled
                    ? ConditionState.NotFulfilled
                    : ConditionState.Fulfilled;
            }

            if (RelatedConditions.Count > 0)
            {
                _relatedConditionsValidator.WithDefaultState(thisState);
                return _relatedConditionsValidator.Validate();
            }

            return thisState;
        }

        public bool IsInverted { get; set; }

        public Condition WithRelatedCondition(ConditionRelation relation, Condition condition)
        {
            if (condition == null) throw new ArgumentNullException(nameof(condition));

            RelatedConditions.Add(new RelatedCondition().WithCondition(condition).WithRelation(relation));
            return this;
        }
        
        public Condition WithExpression(Func<ConditionState> expression)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            return this;
        }

        public Condition WithExpression(Func<bool> expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            Expression = () => expression() ? ConditionState.Fulfilled : ConditionState.NotFulfilled;
            return this;
        }

        public Condition WithInversion()
        {
            IsInverted = true;
            return this;
        }
    }
}
