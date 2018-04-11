using System;
using Wirehome.Contracts.PersonalAgent;

namespace Wirehome.Contracts.ExternalServices.TelegramBot
{
    public class TelegramInboundMessage : MessageBase, IInboundTextMessage
    {
        public TelegramInboundMessage(DateTime timestamp, int chatId, string text)
            : base(chatId, text)
        {
            Timestamp = timestamp;
        }

        public DateTime Timestamp { get; }

        public TelegramOutboundMessage CreateResponse(string text, TelegramMessageFormat format = TelegramMessageFormat.HTML)
        {
            return new TelegramOutboundMessage(ChatId, text, format);
        }
    }
}
