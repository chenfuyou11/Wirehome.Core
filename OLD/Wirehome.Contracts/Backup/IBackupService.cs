using Wirehome.Contracts.Services;
using System;

namespace Wirehome.Contracts.Backup
{
    public interface IBackupService : IService
    {
        event EventHandler<BackupEventArgs> CreatingBackup;
        event EventHandler<BackupEventArgs> RestoringBackup;
    }
}
