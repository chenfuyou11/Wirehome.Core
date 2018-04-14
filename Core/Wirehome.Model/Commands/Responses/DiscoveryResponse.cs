using System.Collections.Generic;
using Wirehome.ComponentModel;
using Wirehome.ComponentModel.Capabilities;

namespace Wirehome.ComponentModel.Commands.Responses
{
    public class DiscoveryResponse : BaseObject
    {
        public DiscoveryResponse(IList<string> requierdProperties, IList<EventSource> eventSources, params State[] supportedStates)
        {
            SupportedStates = supportedStates;
            RequierdProperties = requierdProperties; 
            EventSources = eventSources;
        }

        public DiscoveryResponse(IList<string> requierdProperties, params State[] supportedStates)
        {
            RequierdProperties = requierdProperties;
            SupportedStates = supportedStates;
        }

        public DiscoveryResponse(IList<EventSource> eventSources, params State[] supportedStates)
        {
            EventSources = eventSources;
            SupportedStates = supportedStates;
        }

        public DiscoveryResponse(params State[] supportedStates)
        {
            SupportedStates = supportedStates;
        }

        public State[] SupportedStates { get; }
        public IList<string> RequierdProperties { get; }
        public IList<EventSource> EventSources { get; }
    }
}
