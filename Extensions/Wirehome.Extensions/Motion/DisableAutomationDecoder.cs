using System.Collections.Generic;
using System.Reactive;

namespace Wirehome.Motion
{
    public class DisableAutomationDecoder : IEventDecoder
    {
        private Room _room;

        public void DecodeMessage(IList<Timestamped<PowerStateChangeEvent>> powerStateEvents)
        {
            _room.DisableAutomation();
        }

        public void Init(Room room)
        {
            _room = room;
        }
    }
}