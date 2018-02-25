using Wirehome.Core.EventAggregator;
using Wirehome.Extensions.Messaging.Core;
using Wirehome.Motion.Model;

namespace Wirehome.Extensions.Tests
{
    public class TestHandler : IHandler<MotionEvent>
    {
        public bool IsHandled { get; private set; }

        //[MessageFilter("*")]
        public void Handle(IMessageEnvelope<MotionEvent> message)
        {
            IsHandled = true;
        }
    }
}
