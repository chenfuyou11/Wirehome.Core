using Windows.Storage;
using Wirehome.Contracts.Core;

namespace Wirehome.Raspberry.Core
{
    public class RaspberryStorage : INativeStorage
    {
        public string LocalFolderPath()
        {
            return ApplicationData.Current.LocalFolder.Path;
        }
    }
}
