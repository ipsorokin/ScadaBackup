using ScadaBackup.Models;
using System.Collections.Generic;

namespace ScadaBackup.Controllers
{
    public interface IBackupController
    {
        IEnumerable<BackupFile> GetBackupFiles();
        BackupFile CreateBackup(bool copyEvents, bool copyVoices, bool copyScripts);
        void RestoreBackup(BackupFile file, bool copyEvents, bool copyVoices, bool copyScripts);
        void RemoveBackup(BackupFile file);
    }
}
