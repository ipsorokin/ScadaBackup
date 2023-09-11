using ScadaBackup.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace ScadaBackup.Controllers
{ 
    internal class BackupController : IBackupController
    {
        private readonly string DatabaseFolder = "C:\\1Tekon\\ASUD Scada\\A_JOURNAL";
        private readonly string OPCServerSettingsFolder = "C:\\1Tekon\\ASUD Scada\\OPC Server\\settings";
        private readonly string ScadaFolder = "C:\\1Tekon\\ASUD Scada\\SCADA";
        private readonly string EventsDbFileName = "journal.db";
        private readonly string VoicesDbFileName = "vjm.db";
        private readonly string ZipOPCServerSettings = "OPC Server/settings/";
        private readonly string ZipScadaSettings = "SCADA/settings/";
        private readonly string ZipScadaScripts = "SCADA/scripts/";
        private readonly string ZipDatabases = "A_JOURNAL/";
        private readonly string BackupFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "scada-backups");

        public BackupController()
        {
            if (!Directory.Exists(BackupFolder))
                Directory.CreateDirectory(BackupFolder);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="copyVoices"></param>
        /// <param name="copyEvents"></param>
        /// <param name="copyScripts"></param>
        /// <returns></returns>
        public BackupFile CreateBackup(bool copyEvents, bool copyVoices, bool copyScripts)
        {
            string targetPath = Path.Combine(BackupFolder, GenerateBackupName());
            CopyBaseSettings(targetPath);
            CopyDatabase(targetPath, copyEvents, copyVoices);
            CopyScripts(targetPath, copyScripts);
            return ArchiveCreate(targetPath);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<BackupFile> GetBackupFiles()
        {
            return new DirectoryInfo(BackupFolder).GetFiles().Select(x=> BackupFileAdapter(x));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <exception cref="FileNotFoundException"></exception>
        public void RemoveBackup(BackupFile file)
        {
            if (!File.Exists(file.FullName))
                throw new FileNotFoundException($"Файл не найден.\n{file.Name}");

            File.Delete(file.FullName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="copyVoices"></param>
        /// <param name="copyEvents"></param>
        /// <param name="copyScripts"></param>
        public void RestoreBackup(BackupFile file, bool copyEvents, bool copyVoices, bool copyScripts)
        {
            using (ZipArchive archive = ZipFile.OpenRead(file.FullName))
            {
                ExtractToDirectory(archive, "C:\\1Tekon\\ASUD Scada", copyEvents, copyVoices, copyScripts);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string GenerateBackupName() => Guid.NewGuid().ToString();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private BackupFile BackupFileAdapter(FileInfo file)
        {
            using (ZipArchive archive = ZipFile.OpenRead(file.FullName))
                return new BackupFile(
                    file.Name.Replace(file.Extension, null),
                    file.FullName,
                    file.Length,
                    file.Extension,
                    file.CreationTime,
                    Exists(archive, $"{ZipDatabases}{EventsDbFileName}"),
                    Exists(archive, $"{ZipDatabases}{VoicesDbFileName}"),
                    Exists(archive, $"{ZipScadaScripts}"),
                    GetSdkVersion(archive)
                );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="archive"></param>
        /// <returns></returns>
        private string GetSdkVersion(ZipArchive archive)
        {
            using (Stream stream = archive.GetEntry($"{ZipOPCServerSettings}general.conf").Open())
            {
                XDocument xDocument = XDocument.Load(stream);
                return xDocument.Element("Configuration").Attribute("SDK_Version").Value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetPath"></param>
        private void CopyBaseSettings(string targetPath)
        {
            CopyFilesRecursively(OPCServerSettingsFolder, Path.Combine(targetPath, "OPC Server", "settings"));
            CopyFilesRecursively(Path.Combine(ScadaFolder, "settings"), Path.Combine(targetPath, "SCADA", "settings"));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetPath"></param>
        private void CopyScripts(string targetPath, bool copyScripts)
        {
            if (copyScripts)
                CopyFilesRecursively(Path.Combine(ScadaFolder, "scripts"), Path.Combine(targetPath, "SCADA", "scripts"));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetPath"></param>
        /// <param name="copyVoices"></param>
        /// <param name="copyEvents"></param>
        private void CopyDatabase(string targetPath, bool copyEvents, bool copyVoices)
        {
            string _targetPath = Path.Combine(targetPath, "A_JOURNAL");
            string events_path = Path.Combine(DatabaseFolder, EventsDbFileName);
            string voices_path = Path.Combine(DatabaseFolder, VoicesDbFileName);

            if (copyEvents || copyVoices)
                Directory.CreateDirectory(_targetPath);

            if (copyEvents && File.Exists(events_path))
                File.Copy(events_path, events_path.Replace(DatabaseFolder, _targetPath), true);

            if (copyVoices && File.Exists(voices_path))
                File.Copy(voices_path, voices_path.Replace(DatabaseFolder, _targetPath), true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="targetPath"></param>
        private void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            Directory.CreateDirectory(targetPath);

            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));

            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <returns></returns>
        private BackupFile ArchiveCreate(string sourcePath)
        {
            string targetPath = sourcePath + ".zip";
            ZipFile.CreateFromDirectory(sourcePath, targetPath);
            Directory.Delete(sourcePath, true);
            return BackupFileAdapter(new FileInfo(targetPath));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="archive"></param>
        /// <param name="destinationDirectoryName"></param>
        /// <param name="copyEvents"></param>
        /// <param name="copyVoices"></param>
        /// <param name="copyScripts"></param>
        private void ExtractToDirectory(ZipArchive archive, string destinationDirectoryName, bool copyEvents, bool copyVoices, bool copyScripts)
        {
            foreach (ZipArchiveEntry file in archive.Entries)
            {
                string completeFileName = Path.Combine(destinationDirectoryName, file.FullName);
                string directory = Path.GetDirectoryName(completeFileName);

                if (file.FullName.Contains(ZipScadaSettings) || file.FullName.Contains(ZipOPCServerSettings))
                {
                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);

                    if (file.Name != "")
                        file.ExtractToFile(completeFileName, true);

                    continue;
                }

                if (copyEvents && file.FullName.Contains($"{ZipDatabases}{EventsDbFileName}"))
                {
                    file.ExtractToFile(Path.Combine(DatabaseFolder, EventsDbFileName), true);
                    continue;
                }

                if (copyVoices && file.FullName.Contains($"{ZipDatabases}{VoicesDbFileName}"))
                {
                    file.ExtractToFile(Path.Combine(DatabaseFolder, VoicesDbFileName), true);
                    continue;
                }

                if (copyScripts && file.FullName.Contains(ZipScadaScripts))
                {
                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);

                    if (file.Name != "")
                        file.ExtractToFile(completeFileName, true);

                    continue;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="archive"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        private bool Exists(ZipArchive archive, string target)
        {
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                if (entry.FullName.Contains(target))
                    return true;
            }

            return false;
        }
    }
}
