using System;
using System.IO;
using Wirehome.Contracts.Core;
using Wirehome.Contracts.Logging;
using Wirehome.Contracts.Services;
using Wirehome.Contracts.Storage;
using Newtonsoft.Json;

namespace Wirehome.Storage
{
    public class StorageService : ServiceBase, IStorageService
    {
        private readonly object _syncRoot = new object();
        private readonly ILogger _log;

        public StorageService(ILogService logService)
        {
            _log = logService?.CreatePublisher(nameof(StorageService)) ?? throw new ArgumentNullException(nameof(logService));
        }

        public bool TryRead<TData>(string filename, out TData data)
        {
            if (filename == null) throw new ArgumentNullException(nameof(filename));

            data = default(TData);
            try
            {
                var absoluteFilename = Path.Combine(StoragePath.StorageRoot, filename);
                if (!File.Exists(absoluteFilename))
                {
                    return false;
                }

                string fileContent;
                lock (_syncRoot)
                {
                    fileContent = File.ReadAllText(absoluteFilename);
                }

                if (string.IsNullOrEmpty(fileContent))
                {
                    return false;
                }

                data = JsonConvert.DeserializeObject<TData>(fileContent);
                return true;
            }
            catch (Exception exception)
            {
                _log.Warning(exception, $"Unable to load data from '{filename}'.");
            }

            return false;
        }

        public void Write<TData>(string filename, TData content)
        {
            if (filename == null) throw new ArgumentNullException(nameof(filename));

            var absoluteFilename = Path.Combine(StoragePath.StorageRoot, filename);
            var json = JsonConvert.SerializeObject(content, Formatting.Indented);

            lock (_syncRoot)
            {
                File.WriteAllText(absoluteFilename, json);
            }
        }
    }
}
