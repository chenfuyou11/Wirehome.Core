using Wirehome.Contracts.Core;
using Wirehome.Contracts.Network.Http;

namespace Wirehome.HttpServer
{
    public static class IContainerExtensions
    {
        public static void RegisterHttpServer(this IContainer container)
        {
            container.RegisterSingleton<IHttpServer, HttpServer>();

        }
    }
}

