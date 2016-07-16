using System;
using HA4IoT.Contracts.Hardware;
using System.Collections.Generic;

namespace HA4IoT.Hardware.I2CHardwareBridge
{
    public class ReadLDP433MHzSignalCommandResponse
    {
        public ReadLDP433MHzSignalCommandResponse(IEnumerable<uint> codes)
        {
            Codes = new List<uint>(codes);
        }

        public List<uint> Codes { get; }

    }
}
