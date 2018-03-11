using System.Collections.Generic;
using Wirehome.ComponentModel.Adapters;
using Wirehome.ComponentModel.Components;

namespace Wirehome.ComponentModel.Configuration
{
    public class WirehomeConfiguration
    {
        public IList<Component> Components { get; set; }
        public IList<Adapter> Adapters { get; set; }
    }

    
}
