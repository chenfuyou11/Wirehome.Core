using System.Collections.Generic;
using System.Threading.Tasks;
using Wirehome.ComponentModel.Components;
using Wirehome.Core;

namespace Wirehome.ComponentModel.Adapters
{
    public abstract class Adapter : ComponentBase
    {
        protected readonly List<string> _requierdProperties = new List<string>();

        public IList<string> RequierdProperties() => _requierdProperties;
    }
}