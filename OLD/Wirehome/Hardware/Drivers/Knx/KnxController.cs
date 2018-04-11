using System;
using System.Threading;
using System.Threading.Tasks;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Logging;

namespace Wirehome.Hardware.Drivers.Knx
{
    public class KnxController
    {
        private readonly string _hostName;
        private readonly int _port;
        private readonly string _password;
        private static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        public KnxController(string hostName, int port, string password = "")
        {
            _hostName = hostName ?? throw new ArgumentNullException(nameof(hostName));
            _port = port;
            _password = password;
        }

        public KnxDigitalJoinEnpoint CreateDigitalJoinEndpoint(string identifier)
        {
            return new KnxDigitalJoinEnpoint(identifier, this);
        }

        private async Task Initialization()
        {
            using (var knxClient = new KnxClient(_hostName, _port, _password))
            {
                await knxClient.Connect();
                string response = await knxClient.SendRequestAndWaitForResponse("i=1");

                Log.Default.Verbose("knx-init-answer: " + response);
            }
        }

        public async Task SendDigitalJoinOn(string identifier)
        {
            if (identifier == null) throw new ArgumentNullException(nameof(identifier));

            await semaphoreSlim.WaitAsync().ConfigureAwait(false);
            try
            {
                using (var knxClient = new KnxClient(_hostName, _port, _password))
                {
                    await knxClient.Connect().ConfigureAwait(false);
                    string response = await knxClient.SendRequestAndWaitForResponse(identifier + "=1");

                    Log.Default.Verbose("KnxClient: send-digitalJoinOn: " + response);
                }
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public async Task SendDigitalJoinOff(string identifier)
        {
            if (identifier == null) throw new ArgumentNullException(nameof(identifier));

            await semaphoreSlim.WaitAsync().ConfigureAwait(false);
            try
            {
                using (var knxClient = new KnxClient(_hostName, _port, _password))
                {
                    await knxClient.Connect();
                    string response = await knxClient.SendRequestAndWaitForResponse(identifier + "=0");

                    Log.Default.Verbose("KnxClient: send-digitalJoinOff: " + response);
                }
            }
            finally
            {
                semaphoreSlim.Release();
            }
            
        }

        public void SendAnalogJoin(string identifier, double value)
        {
            ////if (identifier == null) throw new ArgumentNullException(nameof(identifier));

            ////if (value < 0)
            ////    value = 0;

            ////if (value > 65535)
            ////    value = 65535;

            ////string result = _socketClient.Send(identifier + "=" + value + "\x03");
            ////Log.Verbose("knx-send-analogJoin: " + result);
        }

        public void SendSerialJoin(string identifier, string value)
        {
            ////if (identifier == null) throw new ArgumentNullException(nameof(identifier));

            ////string result = _socketClient.Send(identifier + "=" + value + "\x03");
            ////Log.Verbose("knx-send-SerialJoin: " + result);
        }
    }
}
