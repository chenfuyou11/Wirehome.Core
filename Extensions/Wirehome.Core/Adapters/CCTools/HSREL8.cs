using System;
using Wirehome.Core.Adapters.CCTools.Drivers;
using Wirehome.Core.Communication.I2C;

namespace Wirehome.Core.Adapters.CCTools
{
    public class HSREL8Adapter : CCToolsBaseAdapter
    {
        public HSREL8Adapter(II2CBusService i2CBusService, ILogger log) : base(i2CBusService, log) {}
        
        public override void Initialize()
        {
            var address = (IntValue)this[AdapterProperties.I2cAddress];
            this[AdapterProperties.AdapterName] = (StringValue)"CCTools HSREL 8";
            this[AdapterProperties.AdapterAuthor] = (StringValue)"Christian Kratky";
            this[AdapterProperties.AdapterDescription] = (StringValue)"Adapter for a HSRel8(+8) from CCTools.";

            _portExpanderDriver = new MAX7311Driver(new I2CSlaveAddress(address.Value), _i2CBusService);
               
            base.Initialize();

            SetState(new byte[] { 0x00, 255 });
            CommitChanges(true);
        }
    }
}