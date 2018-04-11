using Newtonsoft.Json.Linq;
using System;

namespace Wirehome.Contracts.Actuators
{
    public class StateChangedEventArgs : EventArgs
    {
        public StateChangedEventArgs(JToken oldState, JToken newState)
        {
            OldState = oldState;
            NewState = newState;
        }

        public JToken OldState { get; }

        public JToken NewState { get; }
    }
}
