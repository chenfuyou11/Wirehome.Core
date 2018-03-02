using Wirehome.ComponentModel;
using Wirehome.ComponentModel.Capabilities;

namespace Wirehome.ComponentModel.Commands.Responses
{
    public class DiscoveryResponse : BaseObject
    {
        public DiscoveryResponse(params State[] supportedStates)
        {
            SupportedStates = supportedStates;
        }

        public State[] SupportedStates { get; }
    }
}
