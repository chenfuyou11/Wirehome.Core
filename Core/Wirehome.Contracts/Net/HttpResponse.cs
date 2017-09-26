using System;
using System.Collections.Generic;
using System.Net;

namespace Wirehome.Net.Http
{
    public class HttpResponse
    {
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
        public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public byte[] Body { get; set; } // TODO: Use stream.
        public string MimeType { get; set; }
    }
}