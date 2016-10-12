using System;
using System.Collections.Generic;
using HA4IoT.Contracts.Api;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Services;

namespace HA4IoT.Hardware.RemoteSwitch
{
    public class RemotePilotController : IBinaryInputController
    {
        private readonly object _syncRoot = new object();

        private readonly Dictionary<int, RemoteSocketOutputPort> _ports = new Dictionary<int, RemoteSocketOutputPort>();
        private readonly LPD433MHzSignalReciver _reciver;

        public RemotePilotController(LPD433MHzSignalReciver reciver, ISchedulerService schedulerService)
        {
            if (reciver == null) throw new ArgumentNullException(nameof(reciver));
            if (schedulerService == null) throw new ArgumentNullException(nameof(schedulerService));

            _reciver = reciver;
        }

        public DeviceId Id { get; } = new DeviceId("RemotePilotController");

        //public IBinaryInput GetInput(int number)
        //{
        //    if (number < 0) throw new ArgumentOutOfRangeException(nameof(number));

        //    lock (_syncRoot)
        //    {
        //        RemoteSocketOutputPort output;
        //        if (!_ports.TryGetValue(number, out output))
        //        {
        //            throw new InvalidOperationException("No remote switch with ID " + number + " is registered.");    
        //        }

        //        return output;
        //    }
        //}

        //public RemotePilotController WithRemotePilot(int id, LPD433MHzCodeSequencePair codeSequencePair)
        //{
        //    if (codeSequencePair == null) throw new ArgumentNullException(nameof(codeSequencePair));

        //    lock (_syncRoot)
        //    {
        //        var port = new RemoteSocketOutputPort(id, codeSequencePair, _reciver);
        //        port.Write(BinaryState.Low);

        //        _ports.Add(id, port);
        //    }

        //    return this;
        //}

        public void HandleApiCommand(IApiContext apiContext)
        {
        }

        public void HandleApiRequest(IApiContext apiContext)
        {
        }

        public IBinaryInput GetInput(Int32 number)
        {
            throw new NotImplementedException();
        }
    }
}
