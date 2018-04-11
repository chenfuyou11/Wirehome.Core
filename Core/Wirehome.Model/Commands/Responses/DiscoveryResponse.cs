using System.Collections.Generic;
using Wirehome.ComponentModel;
using Wirehome.ComponentModel.Capabilities;

namespace Wirehome.ComponentModel.Commands.Responses
{
    public class DiscoveryResponse : BaseObject
    {
        public DiscoveryResponse(IList<string> requierdProperties, params State[] supportedStates)
        {
            SupportedStates = supportedStates;
            RequierdProperties = requierdProperties;
        }

        public State[] SupportedStates { get; }
        public IList<string> RequierdProperties { get; }
    }
}
