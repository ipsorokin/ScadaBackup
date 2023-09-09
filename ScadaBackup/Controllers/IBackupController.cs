using ScadaBackup.Models;
using System.Collections.Generic;

namespace ScadaBackup.Controllers
{
    public interface IBackupController
    {
        IEnumerable<BackupFile> GetBackupFiles();
        BackupFile CreateBackup(bool copyVoices, bool copyEvents, bool copyScripts);
        void RestoreBackup(BackupFile file, bool copyVoices, bool copyEvents, bool copyScripts);
        void RemoveBackup(BackupFile file);
    }
}
