using System;
using Wirehome.Conditions;
using Wirehome.Contracts.Hardware;

namespace Wirehome.Conditions.Specialized
{
    public class BinaryInputStateCondition : Condition
    {
        public BinaryInputStateCondition(IBinaryInput input, BinaryState state)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            WithExpression(() => input.Read() == state);
        }
    }
}
