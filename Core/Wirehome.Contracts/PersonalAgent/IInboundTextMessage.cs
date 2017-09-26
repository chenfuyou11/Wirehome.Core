using System;

namespace Wirehome.Contracts.PersonalAgent
{
    public interface IInboundTextMessage
    {
        DateTime Timestamp { get; }

        string Text { get; }
    }
}