using Wirehome.Contracts.Storage;
using System;
using System.Collections.Generic;


namespace Wirehome.Extensions.Tests
{
    public class TestStorageService : IStorageService
    {
        private readonly Dictionary<string, object> _files = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        public void Startup()
        {
        }

        public bool TryRead<TData>(string filename, out TData data)
        {
            object buffer;
            if (!_files.TryGetValue(filename, out buffer))
            {
                data = default(TData);
                return false;
            }

            data = (TData)buffer;
            return true;
        }

        public void Write<TData>(string filename, TData content)
        {
            _files[filename] = content;
        }
    }
}
