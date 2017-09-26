using System;
using Wirehome.Contracts.Components.Adapters;

namespace Wirehome.Tests.Mockups.Adapters
{
    public class TestMotionDetectorAdapter : IMotionDetectorAdapter
    {
        public event EventHandler<MotionDetectorAdapterStateChangedEventArgs> StateChanged;

        public void Refresh()
        {            
        }

        public void Begin()
        {
            StateChanged?.Invoke(this, new MotionDetectorAdapterStateChangedEventArgs(AdapterMotionDetectionState.MotionDetected));
        }

        public void End()
        {
            StateChanged?.Invoke(this, new MotionDetectorAdapterStateChangedEventArgs(AdapterMotionDetectionState.Idle));
        }

        public void Invoke()
        {
            try
            {
                Begin();
            }
            finally
            {
                End();
            }           
        }
    }
}
