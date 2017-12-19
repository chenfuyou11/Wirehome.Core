using Wirehome.Contracts.Components.Adapters;
using System;
using Wirehome.Sensors.MotionDetectors;
using Wirehome.Components;
using Wirehome.Contracts.Sensors;
using Wirehome.Contracts.Messaging;
using Wirehome.Components.Commands;
using Wirehome.Contracts.Settings;
using Wirehome.Contracts.Scheduling;
using Wirehome.Contracts.Components.States;
using Wirehome.Contracts.Components;
using Wirehome.Contracts.Components.Features;

namespace Wirehome.Extensions.Tests
{

    public class TestMotionDetector : ComponentBase, IMotionDetector
    {
        private readonly TestMotionDetectorAdapter _adapter;
        private readonly object _syncRoot = new object();
        private MotionDetectionStateValue _motionDetectionState = MotionDetectionStateValue.Idle;

        public TestMotionDetector(string id, IMotionDetectorAdapter adapter)
            : base(id)
        {
            if (adapter == null)
                throw new ArgumentNullException(nameof(adapter));
            _adapter = (TestMotionDetectorAdapter)adapter;
            adapter.StateChanged += UpdateState;
        }

        public MotionDetectorSettings Settings { get; private set; }

        public override IComponentFeatureCollection GetFeatures()
        {
            return new ComponentFeatureCollection()
                .With(new MotionDetectionFeature());
        }

        public override IComponentFeatureStateCollection GetState()
        {
            return new ComponentFeatureStateCollection()
                .With(new MotionDetectionState(_motionDetectionState));
        }
        

        private void UpdateState(object sender, MotionDetectorAdapterStateChangedEventArgs e)
        {
            var state = e.State == AdapterMotionDetectionState.MotionDetected
                ? MotionDetectionStateValue.MotionDetected
                : MotionDetectionStateValue.Idle;

            lock (_syncRoot)
            {
                 var oldState = GetState();
                _motionDetectionState = state;
                OnStateChanged(oldState);
            }
        }

        public void FakeMove()
        {
            _adapter.FakeMove();
        }

        
    }

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

        public void FakeMove()
        {
            StateChanged?.Invoke(this, new MotionDetectorAdapterStateChangedEventArgs(AdapterMotionDetectionState.MotionDetected));
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
