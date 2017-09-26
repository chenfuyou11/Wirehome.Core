using System;
using Wirehome.Actuators.RollerShutters;
using Wirehome.Automations;
using Wirehome.Contracts.Components;
using Wirehome.Contracts.Components.Commands;
using Wirehome.Contracts.Components.States;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Environment;
using Wirehome.Contracts.Notifications;
using Wirehome.Contracts.Resources;
using Wirehome.Contracts.Scheduling;
using Wirehome.Contracts.Services;
using Wirehome.Contracts.Settings;
using Wirehome.Tests.Mockups;
using Wirehome.Tests.Mockups.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wirehome.Tests.Automations
{
    [TestClass]
    public class RollerShutterAutomationTests
    {
        private TestController _controller;
        private RollerShutter _rollerShutter;
        private TestWeatherStation _weatherStation;
        private RollerShutterAutomation _automation;

        [TestMethod]
        public void SkipOpen_BecauseTooCold()
        {
            Setup();

            _weatherStation.Temperature = 1.5F;
            _automation.Settings.SkipIfFrozenIsEnabled = true;
            _automation.Settings.SkipIfFrozenTemperature = 2;
            _automation.PerformPendingActions();
            _rollerShutter.GetState().Has(PowerState.Off);

            Setup();

            _weatherStation.Temperature = 2.5F;
            _automation.Settings.SkipIfFrozenIsEnabled = true;
            _automation.Settings.SkipIfFrozenTemperature = 2;
            _automation.PerformPendingActions();
            _rollerShutter.GetState().Has(VerticalMovingState.MovingUp);
            _rollerShutter.GetState().Has(PowerState.Off);
        }

        [TestMethod]
        public void Close_BecauseTooHot()
        {
            Setup();
            SkipOpenDueToSunrise();

            _weatherStation.Temperature = 20F;
            _automation.Settings.AutoCloseIfTooHotIsEnabled = true;
            _automation.Settings.AutoCloseIfTooHotTemperaure = 25;
            _automation.PerformPendingActions();
            _rollerShutter.GetState().Has(PowerState.Off);

            _weatherStation.Temperature = 25.5F;
            _automation.PerformPendingActions();
            _rollerShutter.GetState().Has(VerticalMovingState.MovingDown);
            _rollerShutter.GetState().Has(PowerState.On);
        }

        [TestMethod]
        public void Open_AfterSunrise()
        {
            Setup();

            _rollerShutter.GetState().Has(PowerState.Off);
            _automation.PerformPendingActions();
            _rollerShutter.GetState().Has(VerticalMovingState.MovingUp);
            _rollerShutter.GetState().Has(PowerState.On);
        }

        [TestMethod]
        public void Close_AfterSunset()
        {
            Setup();
            SkipOpenDueToSunrise();

            _controller.SetTime(TimeSpan.Parse("18:31"));

            _automation.PerformPendingActions();
            _rollerShutter.GetState().Has(VerticalMovingState.MovingDown);
            _rollerShutter.GetState().Has(PowerState.On);
        }

        private void SkipOpenDueToSunrise()
        {
            _automation.PerformPendingActions();
            _rollerShutter.GetState().Has(VerticalMovingState.MovingUp);
            _rollerShutter.GetState().Has(PowerState.On);
            _rollerShutter.ExecuteCommand(new TurnOffCommand());
        }

        private void Setup()
        {
            _controller = new TestController();
            _controller.SetTime(TimeSpan.Parse("12:00"));

            _weatherStation = new TestWeatherStation { Temperature = 20 };

            _rollerShutter = new RollerShutter("Test", new TestRollerShutterAdapter(), _controller.GetInstance<ITimerService>(), _controller.GetInstance<ISettingsService>());
            _controller.GetInstance<IComponentRegistryService>().RegisterComponent(_rollerShutter);

            _automation = new RollerShutterAutomation(
                "Test",
                _controller.GetInstance<INotificationService>(),
                _controller.GetInstance<ISchedulerService>(),
                _controller.GetInstance<IDateTimeService>(),
                _controller.GetInstance<IDaylightService>(),
                _weatherStation,
                _controller.GetInstance<IComponentRegistryService>(),
                _controller.GetInstance<ISettingsService>(),
                _controller.GetInstance<IResourceService>());

            _automation.WithRollerShutters(_rollerShutter);
        }
    }
}
