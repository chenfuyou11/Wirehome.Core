using System;
using System.Threading.Tasks;

namespace Wirehome.Core.Native
{
    public interface INativeSerialDevice : IDisposable
    {
        Task Init();
        IBinaryReader GetBinaryReader();
    }
}