using System;
using Wirehome.Components;
using Wirehome.Components.Commands;
using Wirehome.Contracts.Components;
using Wirehome.Contracts.Components.Adapters;
using Wirehome.Contracts.Components.Commands;
using Wirehome.Contracts.Components.Features;
using Wirehome.Contracts.Components.States;
using Wirehome.Contracts.Sensors;
using Wirehome.Contracts.Settings;

namespace Wirehome.Sensors.TemperatureSensors
{
    public class TemperatureSensor : ComponentBase, ITemperatureSensor
    {
        private readonly CommandExecutor _commandExecutor = new CommandExecutor();
        private readonly INumericSensorAdapter _adapter;
        private float? _value;

        public TemperatureSensor(string id, INumericSensorAdapter adapter, ISettingsService settingsService)
            : base(id)
        {
            if (settingsService == null) throw new ArgumentNullException(nameof(settingsService));

            _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
            settingsService.CreateSettingsMonitor<SingleValueSensorSettings>(this, s => Settings = s.NewSettings);

            adapter.ValueChanged += (s, e) => Update(e.Value);

            _commandExecutor.Register<ResetCommand>(c => _adapter.Refresh());
        }

        public SingleValueSensorSettings Settings { get; private set; }

        public override IComponentFeatureCollection GetFeatures()
        {
            return new ComponentFeatureCollection()
                .With(new TemperatureMeasurementFeature());
        }

        public override IComponentFeatureStateCollection GetState()
        {
            return new ComponentFeatureStateCollection()
                .With(new TemperatureState(_value));
        }

        public override void ExecuteCommand(ICommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            _commandExecutor.Execute(command);
        }

        private void Update(float? newValue)
        {
            if (!GetDifferenceIsLargeEnough(newValue))
            {
                return;
            }

            var oldState = GetState();
            _value = newValue;
            OnStateChanged(oldState);
        }

        private bool GetDifferenceIsLargeEnough(float? newValue)
        {
            if (_value.HasValue != newValue.HasValue)
            {
                return true;
            }

            if (!_value.HasValue)
            {
                return false;
            }

            if (!newValue.HasValue)
            {
                return false;
            }

            return Math.Abs(_value.Value - newValue.Value) >= Settings.MinDelta;
        }
    }
}