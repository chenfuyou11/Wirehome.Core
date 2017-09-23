using System;
using System.Collections.Generic;
using System.Text;

namespace HA4IoT.Extensions.Contracts
{
    public interface IBinaryReader
    {
        byte ReadByte();
        string ReadString(byte size);
        float ReadSingle();
        uint ReadUInt32();
    }
}
