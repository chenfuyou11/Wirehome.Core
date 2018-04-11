using Wirehome.Contracts.Services;
using System.Collections.Generic;

namespace Wirehome.Contracts.Automations
{
    public interface IAutomationRegistryService : IService
    {
        void AddAutomation(IAutomation automation);

        IList<TAutomation> GetAutomations<TAutomation>() where TAutomation : IAutomation;

        TAutomation GetAutomation<TAutomation>(string id) where TAutomation : IAutomation;

        IList<IAutomation> GetAutomations();
    }
}
