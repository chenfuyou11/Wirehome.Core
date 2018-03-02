using System;
using System.Threading.Tasks;
using Wirehome.ComponentModel.Adapters.Drivers;
using Wirehome.ComponentModel.ValueTypes;
using Wirehome.Core;
using Wirehome.Core.Communication.I2C;
using Wirehome.Core.EventAggregator;

namespace Wirehome.ComponentModel.Adapters
{
    public class HSREL8Adapter : CCToolsBaseAdapter
    {
        public HSREL8Adapter(IEventAggregator eventAggregator, II2CBusService i2CBusService, ILogger log) : base(eventAggregator, i2CBusService, log) {}
        
        public override async Task Initialize()
        {
            var address = (IntValue)this[AdapterProperties.I2cAddress];
            this[AdapterProperties.AdapterName] = (StringValue)"CCTools HSREL 8";
            this[AdapterProperties.AdapterAuthor] = (StringValue)"Christian Kratky";
            this[AdapterProperties.AdapterDescription] = (StringValue)"Adapter for a HSRel8(+8) from CCTools.";

            _portExpanderDriver = new MAX7311Driver(new I2CSlaveAddress(address.Value), _i2CBusService);
               
            await base.Initialize().ConfigureAwait(false);

            SetState(new byte[] { 0x00, 255 });
            CommitChanges(true);
        }
    }
}