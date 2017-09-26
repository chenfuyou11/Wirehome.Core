using System;
using Wirehome.Contracts.PersonalAgent;

namespace Wirehome.Tests.Mockups
{
    public class TestInboundMessage : IInboundTextMessage
    {
        public TestInboundMessage(string text)
        {
            Text = text;
        }

        public DateTime Timestamp { get; set; }
        public string Text { get; }
    }
}
