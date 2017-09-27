using System;

namespace Wirehome.HttpServer.Http
{
    public class BadRequestException : Exception
    {
        public BadRequestException(string message) : base(message)
        {
        }
    }
}
