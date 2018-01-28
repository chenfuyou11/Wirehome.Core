using System.Collections.Generic;
using System.Reactive;

namespace Wirehome.Motion
{
    public interface IEventDecoder
    {
        void DecodeMessage(IList<Timestamped<PowerStateChangeEvent>> powerStateEvents);
        void Init(Room room);
    }
}