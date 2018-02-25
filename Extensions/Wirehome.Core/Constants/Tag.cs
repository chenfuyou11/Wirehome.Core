using System;
using System.Collections.Generic;
using System.Text;

namespace Wirehome.Core.Constants
{
    public static class Tag
    {
        public const string IsOutdoorTemperatureProvider = "is-outdoor-temperature-provider";
        public const string IsOutdoorHumidityProvider = "is-outdoor-humidity-provider";
        public const string IsPrimary = "is-primary";
        public const string IsLamp = "is-lamp";
        public const string IsSocket = "is-socket";
        public const string IsRollerShutter = "is-roller-shutter";
    }


    public static class ModuleType
    {
        public const string Power = "power";
        public const string RollerShutter = "roller-shutter";
        public const string Button = "button";
        public const string Switch = "switch";
        public const string Temperature = "temperature";
        public const string Humidity = "humidity";
        public const string Window = "window";
        public const string Lamp = "lamp";
        public const string Valve = "valve";
        public const string Color = "color";
        public const string Brightness = "brightness";
        public const string Level = "level";
        public const string Motion = "motion";
    }
    public static class PowerModuleProperty
    {
        public const string State = "state";
        public const string Consumption = "consumption";
    }
    public static class AppProperty
    {
        public const string Caption = "app-caption";
        public const string OverviewCaption = "app-overview-caption";
        public const string SortValue = "app-sort-value";
        public const string IsVisible = "app-is-visible";
        public const string Icon = "app-icon";
        public const string IsEnabled = "app-is-enabled";
    }


    //temperature | 22.0 | °C |
    //humidity | 54 | % |
    //hue | 8 | int |
    //saturation | 50 | int |
    //brightness | 100 | %
    //power-consumption | 233 | watts |
    //gas-consumption | 5050,332 | m³ |
    //button-state | released, pressed | enum |
    //valve-state | closed, open | enum
    //window-state | closed, open, halfOpen | enum
    //rollerShutter-state | stopped, movingDown, movingUp | enum
    //rollerShutter-position | 75 | % | 100 % means closed completely


    public static class PowerModuleStateValue
    {
        public const string On = "on";
        public const string Off = "off";
    }
    public static class RollerShutterModuleProperty
    {
        public const string MaxPosition = "max-position";
        public const string Position = "position";
    }
    public static class ColorModuleProperty
    {
        public const string R = "r";
        public const string G = "g";
        public const string B = "b";
    }
    public static class SetLevelCommandProperty
    {
        public const string Level = "level";
    }
    public static class LevelModuleProperty
    {
        public const string MaxLevel = "max-level";
        public const string DefaultLevel = "default-level";
    }
    static class ButtonModuleState
    {
        public const string Released = "released";
        public const string Pressed = "pressed";
    }
}
