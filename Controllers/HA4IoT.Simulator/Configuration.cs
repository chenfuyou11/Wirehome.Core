using System;
using System.Threading.Tasks;
using Wirehome.Actuators.Lamps;
using Wirehome.Actuators.Sockets;
using Wirehome.Components;
using Wirehome.Components.Adapters;
using Wirehome.Contracts;
using Wirehome.Contracts.Actuators;
using Wirehome.Contracts.Areas;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Hardware.DeviceMessaging;
using Wirehome.Contracts.Logging;
using Wirehome.Contracts.Messaging;
using Wirehome.Contracts.Scheduling;
using Wirehome.Contracts.Scripting;
using Wirehome.Contracts.Sensors;
using Wirehome.Contracts.Settings;
using Wirehome.Hardware.Drivers.Outpost;
using Wirehome.Hardware.Drivers.Sonoff;
using Wirehome.Sensors.Buttons;
using Wirehome.Sensors.MotionDetectors;
using Wirehome.Scripting;

namespace Wirehome.Simulator
{
    public class Configuration : IConfiguration
    {
        private readonly MainPage _mainPage;
        private readonly IContainer _containerService;

        public Configuration(MainPage mainPage, IContainer containerService)
        {
            _mainPage = mainPage ?? throw new ArgumentNullException(nameof(mainPage));
            _containerService = containerService ?? throw new ArgumentNullException(nameof(containerService));
        }

        public async Task ApplyAsync()
        {
            var areaRepository = _containerService.GetInstance<IAreaRegistryService>();
            var timerService = _containerService.GetInstance<ITimerService>();
            var settingsService = _containerService.GetInstance<ISettingsService>();
            var deviceMessageBroker = _containerService.GetInstance<IDeviceMessageBrokerService>();
            var schedulerService = _containerService.GetInstance<ISchedulerService>();
            var sonoffDeviceService = _containerService.GetInstance<SonoffDeviceService>();
            var outpostDeviceService = _containerService.GetInstance<OutpostDeviceService>();
            var scriptingService = _containerService.GetInstance<IScriptingService>();
            var messageBroker = _containerService.GetInstance<IMessageBrokerService>();
            var logService = _containerService.GetInstance<ILogService>();

            var area = areaRepository.RegisterArea("TestArea");

            area.RegisterComponent(new Lamp("Lamp1", await _mainPage.CreateUIBinaryOutputAdapter("Lamp 1")));
            area.RegisterComponent(new Lamp("Lamp2", await _mainPage.CreateUIBinaryOutputAdapter("Lamp 2")));
            area.RegisterComponent(new Lamp("Lamp3", await _mainPage.CreateUIBinaryOutputAdapter("Lamp 3")));
            area.RegisterComponent(new Lamp("Lamp4", await _mainPage.CreateUIBinaryOutputAdapter("Lamp 4")));
            area.RegisterComponent(new Lamp("Lamp5", await _mainPage.CreateUIBinaryOutputAdapter("Lamp 5")));

            area.RegisterComponent(new Lamp("RGBS1", outpostDeviceService.CreateRgbStripAdapter("RGBS1")));

            area.RegisterComponent(new Socket("Socket1", await _mainPage.CreateUIBinaryOutputAdapter("Socket 1")));
            area.RegisterComponent(new Socket("Socket2", await _mainPage.CreateUIBinaryOutputAdapter("Socket 2")));
            area.RegisterComponent(new Socket("Socket3", await _mainPage.CreateUIBinaryOutputAdapter("Socket 3")));
            area.RegisterComponent(new Socket("Socket4", await _mainPage.CreateUIBinaryOutputAdapter("Socket 4")));
            area.RegisterComponent(new Socket("Socket5", await _mainPage.CreateUIBinaryOutputAdapter("Socket 5")));

            area.RegisterComponent(new Socket("Socket_POW_01", sonoffDeviceService.GetAdapterForPow("SonoffPow_01")));

            area.RegisterComponent(new Button("Button1", await _mainPage.CreateUIButtonAdapter("Button 1"), timerService, settingsService, messageBroker, logService));
            area.RegisterComponent(new Button("Button2", await _mainPage.CreateUIButtonAdapter("Button 2"), timerService, settingsService, messageBroker, logService));
            area.RegisterComponent(new Button("Button3", await _mainPage.CreateUIButtonAdapter("Button 3"), timerService, settingsService, messageBroker, logService));
            area.RegisterComponent(new Button("Button4", await _mainPage.CreateUIButtonAdapter("Button 4"), timerService, settingsService, messageBroker, logService));
            area.RegisterComponent(new Button("Button5_SONOFF", new VirtualButtonAdapter(), timerService, settingsService, messageBroker, logService));
            area.RegisterComponent(new Button("Button6", await _mainPage.CreateUIButtonAdapter("Button 6"), timerService, settingsService, messageBroker, logService));

            area.RegisterComponent(new MotionDetector("Motion1", await _mainPage.CreateUIMotionDetectorAdapter("Motion Detector 1"),  schedulerService,  settingsService, messageBroker));
            
            area.GetComponent<IButton>("Button1").CreatePressedLongTrigger(messageBroker).Attach(() => area.GetComponent<ILamp>("Lamp2").TryTogglePowerState());

            area.GetComponent<IButton>("Button2").CreatePressedShortTrigger(messageBroker).Attach(() =>
            {
                area.GetComponent<ILamp>("RGBS1").TryTogglePowerState();
            });

            area.GetComponent<IButton>("Button3")
                .CreatePressedShortTrigger(messageBroker)
                .Attach(() => area.GetComponent<ISocket>("Socket1").TryTogglePowerState());

            area.GetComponent<IButton>("Button4")
                .CreatePressedShortTrigger(messageBroker)
                .Attach(() => area.GetComponent<ISocket>("Socket2").TryTogglePowerState());

            area.GetComponent<IButton>("Button5_SONOFF")
                .CreatePressedShortTrigger(messageBroker)
                .Attach(() => area.GetComponent<ISocket>("Socket_POW_01").TryTogglePowerState());

            ScriptExecutionResult result;
            area.GetComponent<IButton>("Button6").CreatePressedShortTrigger(messageBroker).Attach(() => scriptingService.TryExecuteScript("return 'Hello World'", out result));
        }
    }
}
