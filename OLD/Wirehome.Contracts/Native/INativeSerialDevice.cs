using System;
using System.Threading.Tasks;

namespace Wirehome.Contracts.Core
{
    public interface INativeSerialDevice : IDisposable
    {
        Task Init();
        IBinaryReader GetBinaryReader();
    }
}