using System.Linq;
using Wirehome.ComponentModel.Capabilities.Constants;
using Wirehome.ComponentModel.Commands;
using Wirehome.ComponentModel.Components;
using Wirehome.ComponentModel.ValueTypes;

namespace Wirehome.ComponentModel.Capabilities
{
    public class State : BaseObject
    {
        public AdapterReference Adapter { get; private set; }

        public void SetAdapterReference(AdapterReference adapter) => Adapter = adapter;
        
        public State()
        {
            this[StateProperties.TimeOfValue] = new DateTimeValue();
        }

        public bool IsCommandSupported(Command command) => ((StringListValue)this[StateProperties.SupportedCommands]).Value.Contains(command.Type);

    }

}
