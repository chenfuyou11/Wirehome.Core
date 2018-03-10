using System.Collections.Generic;
using Wirehome.ComponentModel.Components;

namespace Wirehome.ComponentModel.Configuration
{
    public interface IConfigurationService
    {
        IList<Component> ReadConfiguration(string rawConfig);
    }
}