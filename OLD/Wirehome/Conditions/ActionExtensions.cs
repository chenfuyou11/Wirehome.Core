using System;
using Wirehome.Contracts.Core;

namespace Wirehome.Conditions
{
    public static class ActionExtensions
    {
        public static ConditionalAction ToConditionalAction(this IAction action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            return new ConditionalAction().WithAction(action);
        }
    }
}
