using Wirehome.ComponentModel.Capabilities.Constants;
using Wirehome.ComponentModel.Commands;
using Wirehome.ComponentModel.Component;
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
    }

}
