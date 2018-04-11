using System;

namespace Wirehome.Contracts.Settings
{
    public class SettingsChangedEventArgs<TSettings> : EventArgs
    {
        public SettingsChangedEventArgs(TSettings oldSettings, TSettings newSettings)
        {
            OldSettings = oldSettings;
            NewSettings = newSettings;
        }

        public TSettings OldSettings { get; set; }

        public TSettings NewSettings { get; set; }
    }
}
