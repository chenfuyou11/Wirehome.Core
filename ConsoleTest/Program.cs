using System;
using System.Threading.Tasks;
using Wirehome.ComponentModel.Adapters;
using Wirehome.ComponentModel.Component;
using Wirehome.ComponentModel.ValueTypes;
using Wirehome.Core;
using Wirehome.Core.Communication.I2C;
using Wirehome.Core.EventAggregator;

namespace ConsoleTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            try
            {
                var eventAggregator = new EventAggregator();
                var component = new Component(eventAggregator);

                var i2cServiceBus = new I2C();
                var logger = new Logger();
                //Mock.Get(daylightService).Setup(x => x.Sunrise).Returns(TimeSpan.FromHours(8));

                var adapter = new HSREL8Adapter(eventAggregator, i2cServiceBus, logger)
                {
                    Uid = "HSREL8Adapter"
                };
                //adapter[AdapterProperties.PinNumber] = new IntValue(1);
                adapter.SetPropertyValue("Test", new IntValue(1));

                //component.AddAdapter(adapter.Uid);

                //await component.Initialize().ConfigureAwait(false);

                Console.ReadLine();

            }
            catch (System.Exception ee)
            {

                Console.WriteLine(ee.ToString());
            }

        }

    }

    public class I2C : II2CBusService
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task Initialize()
        {
            throw new NotImplementedException();
        }

        public II2CTransferResult Read(I2CSlaveAddress address, byte[] buffer, bool useCache = true)
        {
            throw new NotImplementedException();
        }

        public II2CTransferResult Write(I2CSlaveAddress address, byte[] buffer, bool useCache = true)
        {
            throw new NotImplementedException();
        }

        public II2CTransferResult WriteRead(I2CSlaveAddress address, byte[] writeBuffer, byte[] readBuffer, bool useCache = true)
        {
            throw new NotImplementedException();
        }
    }

    public class Logger : ILogger
    {
        public void Error(string message)
        {
            throw new NotImplementedException();
        }

        public void Error(Exception exception, string message)
        {
            throw new NotImplementedException();
        }

        public void Info(string message)
        {
            throw new NotImplementedException();
        }

        public void Publish(LogEntrySeverity severity, string message, Exception exception)
        {
            throw new NotImplementedException();
        }

        public void Verbose(string message)
        {
            throw new NotImplementedException();
        }

        public void Warning(string message)
        {
            throw new NotImplementedException();
        }

        public void Warning(Exception exception, string message)
        {
            throw new NotImplementedException();
        }
    }
}
