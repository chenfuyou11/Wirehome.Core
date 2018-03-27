namespace Wirehome.ComponentModel.Commands
{
    public static class CommandType
    {
        public const string DiscoverCapabilities = "DiscoverCapabilities";
        public const string RefreshCommand = "RefreshCommand";

        // Power
        public const string TurnOffCommand = "TurnOffCommand";

        public const string TurnOnCommand = "TurnOnCommand";
        public const string SwitchPowerCommand = "SwitchPowerCommand";

        // Volume
        public const string VolumeUpCommand = "VolumeUpCommand";

        public const string VolumeDownCommand = "VolumeDownCommand";
        public const string VolumeSetCommand = "VolumeSetCommand";

        public const string MuteCommand = "MuteCommand";
        public const string UnmuteCommand = "UnmuteCommand";

        // Obsolate?
        public const string QueryCommand = "QueryCommand";

        public const string UpdateCommand = "UpdateCommand";
    }
}