using Wirehome.Contracts.Services;

namespace Wirehome.Contracts.ExternalServices.TelegramBot
{
    public interface ITelegramBotService : IService
    {
        void EnqueueMessage(TelegramOutboundMessage message);

        void EnqueueMessageForAdministrators(string text, TelegramMessageFormat format = TelegramMessageFormat.HTML);
    }
}