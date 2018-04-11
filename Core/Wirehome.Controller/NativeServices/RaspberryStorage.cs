using Windows.Storage;
using Wirehome.Core.Interface.Native;

namespace Wirehome.Raspberry.Core
{
    public class RaspberryStorage : INativeStorage
    {
        public string LocalFolderPath() => ApplicationData.Current.LocalFolder.Path;
    }
}
