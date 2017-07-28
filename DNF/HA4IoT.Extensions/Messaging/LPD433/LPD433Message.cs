namespace HA4IoT.Extensions.Messaging
{
    public class LPD433Message
    {
        public uint Code { get; set; }
        public uint Bits { get; set; }
        public uint Protocol { get; set; }

        public override string ToString()
        {
            return $"Code: {Code}, Bits: {Bits}, Protocol: {Protocol}";
        }
    }
}
