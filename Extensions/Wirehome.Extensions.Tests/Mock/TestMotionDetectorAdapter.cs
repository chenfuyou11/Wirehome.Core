using Wirehome.Contracts.Components.Adapters;
using System;


namespace Wirehome.Extensions.Tests
{
    public class TestMotionDetectorAdapter : IMotionDetectorAdapter
    {
        public event EventHandler MotionDetectionBegin;
        public event EventHandler MotionDetectionEnd;
        public event EventHandler<MotionDetectorAdapterStateChangedEventArgs> StateChanged;

        public TestMotionDetectorAdapter()
        {
            StateChanged?.Invoke(null, null);
        }

        public void Refresh()
        {
        }

        public void Begin()
        {
            MotionDetectionBegin?.Invoke(this, EventArgs.Empty);
        }

        public void End()
        {
            MotionDetectionEnd?.Invoke(this, EventArgs.Empty);
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
