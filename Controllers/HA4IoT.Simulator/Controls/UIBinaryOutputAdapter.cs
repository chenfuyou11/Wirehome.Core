using System;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Wirehome.Contracts.Components.Adapters;
using Wirehome.Contracts.Hardware;

namespace Wirehome.Simulator.Controls
{
    public class UIBinaryOutputAdapter : IBinaryOutputAdapter, ILampAdapter
    {
        private readonly CheckBox _checkBox;

        public UIBinaryOutputAdapter(CheckBox checkBox)
        {
            _checkBox = checkBox ?? throw new ArgumentNullException(nameof(checkBox));
        }

        public bool SupportsColor => false;
        public int ColorResolutionBits => 0;

        public async Task SetState(AdapterPowerState powerState, params IHardwareParameter[] parameters)
        {
            await _checkBox.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                () =>
                {
                    _checkBox.IsChecked = powerState == AdapterPowerState.On;
                });
        }
        
        public async Task SetState(AdapterPowerState powerState, AdapterColor color, params IHardwareParameter[] hardwareParameters)
        {
            await _checkBox.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                () =>
                {
                    _checkBox.IsChecked = powerState == AdapterPowerState.On;
                });
        }
    }
}
