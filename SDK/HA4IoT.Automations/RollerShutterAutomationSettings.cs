﻿using System;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Automations;
using HA4IoT.Contracts.Logging;
using HA4IoT.Core.Settings;

namespace HA4IoT.Automations
{
    public class RollerShutterAutomationSettings : AutomationSettings
    {
        public RollerShutterAutomationSettings(AutomationId automationId, IApiController apiController, ILogger logger) 
            : base(automationId, apiController, logger)
        {
            SkipBeforeTimestampIsEnabled = new Setting<bool>(false);
            SkipBeforeTimestamp = new Setting<TimeSpan>(TimeSpan.Parse("07:15"));

            AutoCloseIfTooHotIsEnabled = new Setting<bool>(false);
            AutoCloseIfTooHotTemperaure = new Setting<float>(25);

            SkipIfRollerShutterFrozenIsEnabled = new Setting<bool>(true);
            SkipIfRollerShutterFrozenTemperature = new Setting<float>(2);

            OpenOnSunriseOffset = new Setting<TimeSpan>(TimeSpan.FromMinutes(-30));
            CloseOnSunsetOffset = new Setting<TimeSpan>(TimeSpan.FromMinutes(30));
        }

        public Setting<bool> SkipBeforeTimestampIsEnabled { get; private set; } 

        public Setting<TimeSpan> SkipBeforeTimestamp { get; private set; }

        public Setting<bool> AutoCloseIfTooHotIsEnabled { get; private set; } 

        public Setting<float> AutoCloseIfTooHotTemperaure { get; private set; }

        public Setting<bool> SkipIfRollerShutterFrozenIsEnabled { get; private set; }

        public Setting<float> SkipIfRollerShutterFrozenTemperature { get; private set; }

        public Setting<TimeSpan> OpenOnSunriseOffset { get; private set; } 

        public Setting<TimeSpan> CloseOnSunsetOffset { get; private set; } 
    }
}
