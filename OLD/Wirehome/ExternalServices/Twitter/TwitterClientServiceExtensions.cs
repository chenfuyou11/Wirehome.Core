using System;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.ExternalServices.Twitter;

namespace Wirehome.ExternalServices.Twitter
{
    public static class TwitterClientServiceExtensions
    {
        public static IAction GetTweetAction(this ITwitterClientService twitterClientService, string message)
        {
            if (twitterClientService == null) throw new ArgumentNullException(nameof(twitterClientService));
            if (message == null) throw new ArgumentNullException(nameof(message));

            return new TweetAction(message, twitterClientService);
        }

        public static IAction GetTweetAction(this ITwitterClientService twitterClientService, Func<string> messageProvider)
        {
            if (twitterClientService == null) throw new ArgumentNullException(nameof(twitterClientService));
            if (messageProvider == null) throw new ArgumentNullException(nameof(messageProvider));

            return new TweetAction(messageProvider, twitterClientService);
        }
    }
}
