using System;
using System.Collections.Generic;
using System.Text;

namespace Wirehome.Contracts.Core
{
    public interface INativeTimerSerice
    {
        void CreatePeriodicTimer(Action action, TimeSpan interval);
    }
}
