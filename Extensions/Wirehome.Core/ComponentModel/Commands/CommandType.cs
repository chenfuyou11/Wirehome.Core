namespace Wirehome.ComponentModel.Commands
{
    public static class CommandType
    {
        public const string TurnOff = "turn-off";
        public const string TurnOn = "turn-on";
        public const string SwitchPowerState = "switch-power-state";

        public const string MoveUp = "move-up";
        public const string MoveDown = "move-down";
        public const string Open = "open";
        public const string Close = "close";
        public const string SetLevel = "set-level";
        public const string IncreaseLevel = "increase-level";
        public const string DecreaseLevel = "decrease-level";
        public const string SetBrightness = "set-brightness";
        public const string SetColor = "set-color";

        public const string QueryCommand = "QueryCommand";
        public const string UpdateCommand = "UpdateCommand";
        public const string DiscoverCapabilities = "DiscoverCapabilities";
        public const string RefreshCommand = "RefreshCommand";
    }
}