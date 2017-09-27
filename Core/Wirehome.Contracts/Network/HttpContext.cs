using System;

namespace Wirehome.Contracts.Network.Http
{
    public class HttpContext
    {
        public HttpContext(HttpRequest request, HttpResponse response)
        {
            Request = request ?? throw new ArgumentNullException(nameof(request));
            Response = response ?? throw new ArgumentNullException(nameof(response));
        }

        public HttpRequest Request { get; }
        public HttpResponse Response { get; }
    }
}