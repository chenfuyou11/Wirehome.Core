namespace HA4IoT.Extensions
{
    public class InfraredMessage : Message
    {
        public byte System { get; set; }
        public uint Code { get; set; }
        public byte Bits { get; set; }

        public override string ToString()
        {
            return $"System: {IfraredSystem}, Code: {Code}, Bits: {Bits}";
        }

        public IfraredSystem IfraredSystem
        {
            get
            {
                return (IfraredSystem)System;
            }
            set
            {
                System = (byte)value;
            }
        }
    }



}
