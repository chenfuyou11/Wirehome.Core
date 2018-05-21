using System.Threading.Tasks;

namespace Wirehome.ComponentModel.Configuration
{
    public interface IConfigurationService
    {
        Task<WirehomeConfiguration> ReadConfiguration(string configPath, string adaptersRepoPath);
    }
}