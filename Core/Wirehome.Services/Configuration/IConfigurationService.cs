using System.Threading.Tasks;

namespace Wirehome.ComponentModel.Configuration
{
    public interface IConfigurationService
    {
        Task<WirehomeConfiguration> ReadConfiguration(string rawConfig, string adaptersRepoPath);
    }
}