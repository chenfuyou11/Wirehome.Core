using System;
using System.Collections.Generic;
using System.Text;
using Wirehome.ComponentModel;
using Wirehome.ComponentModel.Commands;
using Wirehome.ComponentModel.Events;

namespace Wirehome.ComponentModel.Components
{
    public class Trigger
    {
        public Event Event { get; set; }
        public Command Command { get; set; }
    }
}
