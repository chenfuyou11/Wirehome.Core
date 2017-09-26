using System;
using Wirehome.Contracts.Services;

namespace Wirehome.Contracts.Core
{
    public interface ISystemInformationService : IService
    {
        void Set(string key, object value);

        void Set(string key, Func<object> valueProvider);

        void Delete(string key);
    }
}
