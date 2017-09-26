using System.Threading.Tasks;
using Wirehome.Contracts.Components.Adapters;
using Wirehome.Contracts.Hardware;

namespace Wirehome.Tests.Mockups.Adapters
{
    public class TestBinaryOutputAdapter : IBinaryOutputAdapter
    {
        public int TurnOnCalledCount { get; set; }
        public int TurnOffCalledCount { get; set; }
        
        public Task SetState(AdapterPowerState powerState, params IHardwareParameter[] parameters)
        {
            if (powerState == AdapterPowerState.On) TurnOnCalledCount++;
            if (powerState == AdapterPowerState.Off) TurnOffCalledCount++;

            return Task.FromResult(0);
        }
    }
}
