using System.Collections.Generic;

namespace Wirehome.Contracts.Components.Features
{
    public class StateMachineFeature : IComponentFeature
    {
        public HashSet<string> SupportedStates { get; } = new HashSet<string>();
    }
}
