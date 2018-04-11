using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet.Core.Adapter;
using MQTTnet.Core.Packets;
using MQTTnet.Core.Serializer;

namespace Wirehome.Devices
{
    public class MqttCommunicationAdapter : IMqttCommunicationAdapter
    {
        private readonly BlockingCollection<MqttBasePacket> _incomingPackets = new BlockingCollection<MqttBasePacket>();

        public MqttCommunicationAdapter Partner { get; set; }

        public IMqttPacketSerializer PacketSerializer => throw new NotImplementedException();
        
        private void SendPacketInternal(MqttBasePacket packet)
        {
            if (packet == null) throw new ArgumentNullException(nameof(packet));

            _incomingPackets.Add(packet);
        }

        private void ThrowIfPartnerIsNull()
        {
            if (Partner == null)
            {
                throw new InvalidOperationException("Partner is not set.");
            }
        }

        public Task ConnectAsync(TimeSpan timeout)
        {
            return Task.FromResult(0);
        }

        public Task DisconnectAsync(TimeSpan timeout)
        {
            return Task.FromResult(0);
        }

        public async Task SendPacketsAsync(TimeSpan timeout, CancellationToken cancellationToken, IEnumerable<MqttBasePacket> packets)
        {
            ThrowIfPartnerIsNull();

            //TODO Test after migrate - add timeout
            foreach (var packet in packets)
            {
                cancellationToken.ThrowIfCancellationRequested();
                Partner.SendPacketInternal(packet);
            }
            await Task.FromResult(0).ConfigureAwait(false);
        }

        public Task<MqttBasePacket> ReceivePacketAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
            // TODOadd timeout and token support
            ThrowIfPartnerIsNull();

            return Task.Run(() => _incomingPackets.Take());
        }

       
    }
}
