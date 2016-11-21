namespace HA4IoT.Hardware.I2CHardwareBridge
{
    public class ReadCurrentSensorCommandResponse
    {
        public ReadCurrentSensorCommandResponse(bool succeeded, float current)
        {
            Succeeded = succeeded;
            Current = current;
        }

        public bool Succeeded { get; }

        public float Current { get; }

    }
}
