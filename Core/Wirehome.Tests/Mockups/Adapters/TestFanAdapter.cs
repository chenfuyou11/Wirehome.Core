using Wirehome.Contracts.Components.Adapters;
using Wirehome.Contracts.Hardware;

namespace Wirehome.Tests.Mockups.Adapters
{
    public class TestFanAdapter : IFanAdapter
    {
        public int MaxLevel { get; set; }

        public int CurrentLevel { get; set; }

        public void SetState(int level, params IHardwareParameter[] parameters)
        {
            CurrentLevel = level;
        }
    }
}
