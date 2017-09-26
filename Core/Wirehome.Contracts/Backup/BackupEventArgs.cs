using Newtonsoft.Json.Linq;
using System;

namespace Wirehome.Contracts.Backup
{
    public class BackupEventArgs : EventArgs
    {
        public BackupEventArgs(JObject backup)
        {
            if (backup == null) throw new ArgumentNullException(nameof(backup));

            Backup = backup;
        }

        public JObject Backup { get; }
    }
}
