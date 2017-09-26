using System.Collections.Generic;

namespace Wirehome.Contracts.Hardware.RemoteSockets.Configuration
{
    public sealed class RemoteSocketServiceConfiguration
    {
        public Dictionary<string, RemoteSocketConfiguration> RemoteSockets = new Dictionary<string, RemoteSocketConfiguration>();
    }
}
