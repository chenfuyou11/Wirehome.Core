namespace Wirehome.ComponentModel.Commands
{
    public static class CommandType
    {
        public const string DiscoverCapabilities = "DiscoverCapabilities";

        //Refresh full device state
        public const string RefreshCommand = "RefreshCommand";

        // Refresh only part of the states
        public const string RefreshLightCommand = "RefreshLightCommand";

        // Power
        public const string TurnOffCommand = "TurnOffCommand";

        public const string TurnOnCommand = "TurnOnCommand";
        public const string SwitchPowerCommand = "SwitchPowerCommand";

        // Volume
        public const string VolumeUpCommand = "VolumeUpCommand";

        public const string VolumeDownCommand = "VolumeDownCommand";
        public const string VolumeSetCommand = "VolumeSetCommand";

        //Mute
        public const string MuteCommand = "MuteCommand";

        public const string UnmuteCommand = "UnmuteCommand";

        // Input
        public const string SelectInputCommand = "SelectInputCommand";

        // Surround
        public const string SelectSurroundModeCommand = "SelectSurroundModeCommand";

        // Obsolate?
        public const string QueryCommand = "QueryCommand";

        public const string UpdateCommand = "UpdateCommand";
    }
}