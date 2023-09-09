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
        private static string DatabaseFolder { get; } = "C:\\1Tekon\\ASUD Scada\\A_JOURNAL";
        private static string OPCServerFolder { get; } = "C:\\1Tekon\\ASUD Scada\\OPC Server";
        private static string ScadaFolder { get; } = "C:\\1Tekon\\ASUD Scada\\SCADA";
        private static string BackupFolder { get; } = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "scada-backups");

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
        public BackupFile CreateBackup(bool copyVoices, bool copyEvents, bool copyScripts)
        {
            string targetPath = Path.Combine(BackupFolder, GenerateBackupName());
            CopyBaseSettings(targetPath);
            CopyDatabase(targetPath, copyVoices, copyEvents);
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
        public void RestoreBackup(BackupFile file, bool copyVoices, bool copyEvents, bool copyScripts)
        {
            ArchiveExtract(file);
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
                    Exists(archive, "A_JOURNAL/journal.db"),
                    Exists(archive, "A_JOURNAL/vjm.db"),
                    Exists(archive, "SCADA/scripts/"),
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
            using (Stream stream = archive.GetEntry("OPC Server/settings/general.conf").Open())
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
            CopyFilesRecursively(Path.Combine(OPCServerFolder, "settings"), Path.Combine(targetPath, "OPC Server", "settings"));
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
        private void CopyDatabase(string targetPath, bool copyVoices, bool copyEvents)
        {
            string _targetPath = Path.Combine(targetPath, "A_JOURNAL");
            string voices_path = Path.Combine(DatabaseFolder, "vjm.db");
            string events_path = Path.Combine(DatabaseFolder, "journal.db");

            if (copyEvents || copyVoices)
                Directory.CreateDirectory(_targetPath);

            if (copyVoices && File.Exists(voices_path))
                File.Copy(voices_path, voices_path.Replace(DatabaseFolder, _targetPath), true);

            if (copyEvents && File.Exists(events_path))
                File.Copy(events_path, events_path.Replace(DatabaseFolder, _targetPath), true);
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
        /// <param name="file"></param>
        private void ArchiveExtract(BackupFile file)
        {
            using (ZipArchive archive = ZipFile.OpenRead(file.FullName))
                ExtractToDirectory(archive, "C:\\1Tekon\\ASUD Scada", true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="archive"></param>
        /// <param name="destinationDirectoryName"></param>
        /// <param name="overwrite"></param>
        private void ExtractToDirectory(ZipArchive archive, string destinationDirectoryName, bool overwrite)
        {
            if (!overwrite)
            {
                archive.ExtractToDirectory(destinationDirectoryName);
                return;
            }

            foreach (ZipArchiveEntry file in archive.Entries)
            {
                string completeFileName = Path.Combine(destinationDirectoryName, file.FullName);
                string directory = Path.GetDirectoryName(completeFileName);

                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                if (file.Name != "")
                    file.ExtractToFile(completeFileName, true);
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
