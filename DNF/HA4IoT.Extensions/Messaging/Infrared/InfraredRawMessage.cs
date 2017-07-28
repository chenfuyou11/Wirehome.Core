using System.Collections.Generic;

namespace HA4IoT.Extensions.Messaging
{
    public class InfraredRawMessage
    {
        public List<ushort> RawArray { get; set; } = new List<ushort>();

        public override string ToString()
        {
            return $"Raw message of size {RawArray.Count}";
        }
    }
}
