using System;
using Wirehome.Contracts.ExternalServices.Twitter;
using Wirehome.Contracts.Scripting;
using MoonSharp.Interpreter;

namespace Wirehome.ExternalServices.Twitter
{
    public class TwitterClientScriptProxy : IScriptProxy
    {
        private readonly ITwitterClientService _twitterClientService;

        [MoonSharpHidden]
        public TwitterClientScriptProxy(ITwitterClientService twitterClientService)
        {
            _twitterClientService = twitterClientService ?? throw new ArgumentNullException(nameof(twitterClientService));
        }

        [MoonSharpHidden]
        public string Name => "twitter";

        public void Tweet(string message)
        {
            _twitterClientService.TryTweet(message);
        }
    }
}
