using System.Threading.Tasks;

namespace Wirehome.ComponentModel.Configuration
{
    public interface IConfigurationService
    {
        WirehomeConfiguration ReadConfiguration(string configPath, string adaptersRepoPath);
    }
}