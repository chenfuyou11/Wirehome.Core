namespace HA4IoT.Extensions
{
    public class InfraredMessage
    {
        public byte System { get; set; }
        public uint Code { get; set; }
        public byte Bits { get; set; }

        public override string ToString()
        {
            return $"System: {System}, Code: {Code}, Bits: {Bits}";
        }
    }
}
