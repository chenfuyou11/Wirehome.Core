using Wirehome.Contracts.Components.States;

namespace Wirehome.Motion
{
    public class PowerStateChangeEvent
    {
        public static string ManualSource = "Manual";
        public static string AutoSource = "Auto";

        public PowerStateChangeEvent(PowerStateValue value, string eventSource)
        {
            Value = value;
            EventSource = eventSource;
        }

        public PowerStateValue Value { get;}
        public string EventSource { get; }
    }

}