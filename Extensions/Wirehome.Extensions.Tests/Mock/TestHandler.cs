using Wirehome.Extensions.Messaging.Core;

namespace Wirehome.Extensions.Tests
{
    public class TestHandler : IHandler<MotionEvent>
    {
        public bool IsHandled { get; private set; }

        public void Handle(IMessageEnvelope<MotionEvent> message)
        {
            IsHandled = true;
        }
    }
}
