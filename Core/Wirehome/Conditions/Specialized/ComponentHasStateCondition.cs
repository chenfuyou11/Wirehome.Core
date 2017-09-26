using System;
using Wirehome.Contracts.Components;

namespace Wirehome.Conditions.Specialized
{
    public class ComponentHasStateCondition : Condition
    {
        public ComponentHasStateCondition(IComponent component, IComponentFeatureState state)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            if (state == null) throw new ArgumentNullException(nameof(state));
            
            WithExpression(() => component.GetState().Has(state));
        }        
    }
}
