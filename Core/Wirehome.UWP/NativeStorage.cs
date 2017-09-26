using Windows.Storage;
using Wirehome.Contracts.Core;

namespace Wirehome.UWP.Core
{
    public class NativeStorage : INativeStorage
    {
        public string LocalFolderPath()
        {
            return ApplicationData.Current.LocalFolder.Path;
        }
    }
}
