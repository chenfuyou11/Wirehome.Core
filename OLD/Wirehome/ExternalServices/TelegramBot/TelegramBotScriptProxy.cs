using System;
using Wirehome.Contracts.ExternalServices.TelegramBot;
using Wirehome.Contracts.Scripting;
using MoonSharp.Interpreter;

namespace Wirehome.ExternalServices.TelegramBot
{
    public class TelegramBotScriptProxy : IScriptProxy
    {
        private readonly ITelegramBotService _telegramBotService;

        [MoonSharpHidden]
        public TelegramBotScriptProxy(ITelegramBotService telegramBotService)
        {
            _telegramBotService = telegramBotService ?? throw new ArgumentNullException(nameof(telegramBotService));
        }

        [MoonSharpHidden]
        public string Name => "telegramBot";

        public void SendAdminMessage(string message)
        {
            _telegramBotService.EnqueueMessageForAdministrators(message);
        }
    }
}
