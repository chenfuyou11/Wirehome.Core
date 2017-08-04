using System;

namespace HA4IoT.Extensions.Devices
{
    public class BraviaApiException : Exception
    {
        public int ErrorId { get; }
        public BraviaApiException(int errorId, string message)
            : base(message)
        {
            this.ErrorId = errorId;
        }
    }
    
}
