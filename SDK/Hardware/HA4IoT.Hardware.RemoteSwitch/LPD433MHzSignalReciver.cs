using System;
using System.Linq;
using HA4IoT.Contracts.Api;
using HA4IoT.Hardware.I2CHardwareBridge;
using HA4IoT.Hardware.RemoteSwitch.Codes;

namespace HA4IoT.Hardware.RemoteSwitch
{
    public class LPD433MHzSignalReciver
    {
        private readonly I2CHardwareBridge.I2CHardwareBridge _i2CHardwareBridge;
        private readonly byte _pin;

        public event EventHandler<BrennenstuhlCode> SignalRecived;

        public LPD433MHzSignalReciver(I2CHardwareBridge.I2CHardwareBridge i2CHardwareBridge, byte pin, IApiController apiController)
        {
            if (i2CHardwareBridge == null) throw new ArgumentNullException(nameof(i2CHardwareBridge));
            if (apiController == null) throw new ArgumentNullException(nameof(apiController));

            _i2CHardwareBridge = i2CHardwareBridge;
            _pin = pin;

            _i2CHardwareBridge.LDP433Accessor.InitSensor(pin);
            _i2CHardwareBridge.LDP433Accessor.ValuesArrived += LDP433Accessor_ValuesArrived;

            //TODO API POST
        }

        private void LDP433Accessor_ValuesArrived(object sender, ReadLDP433MHzSignalCommandResponse e)
        {
            var codes = e.Codes.Select(x => BrennenstuhlCode.ParseCode(x))
                               .Where(y => y != null)
                               .Distinct()
                               .ToList();

            codes.ForEach(x => SignalRecived?.Invoke(this, x));
        }


    }
}
