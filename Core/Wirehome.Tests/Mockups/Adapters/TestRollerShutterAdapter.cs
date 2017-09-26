using System.Threading.Tasks;
using Wirehome.Contracts.Components.Adapters;
using Wirehome.Contracts.Hardware;

namespace Wirehome.Tests.Mockups.Adapters
{
    public class TestRollerShutterAdapter : IRollerShutterAdapter
    {
        public int StartMoveUpCalledCount { get; set; }

        public int StopCalledCount { get; set; }

        public int StartMoveDownCalledCount { get; set; }

        public Task SetState(AdapterRollerShutterState state, params IHardwareParameter[] parameters)
        {
            if (state == AdapterRollerShutterState.Stop) StopCalledCount++;
            if (state == AdapterRollerShutterState.MoveUp) StartMoveUpCalledCount++;
            if (state == AdapterRollerShutterState.MoveDown) StartMoveDownCalledCount++;

            return Task.FromResult(0);
        }
    }
}
