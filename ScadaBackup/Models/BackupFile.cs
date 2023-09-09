using System;

namespace ScadaBackup.Models
{
    public class BackupFile
    {
        public BackupFile(string name, string fullName, long length, string extension, DateTime creationTime)
        {
            Name = name;
            FullName = fullName;
            Length = length;
            Extension = extension;
            CreationTime = creationTime;
        }

        public BackupFile(string name, string fullName, long length, string extension, DateTime creationTime, bool eventsDbExists, bool voicesDbExists, bool scriptsDbExists) : this(name, fullName, length, extension, creationTime)
        {
            EventsDatabaseExists = eventsDbExists;
            VoicesDatabaseExists = voicesDbExists;
            ScriptsFolderExists = scriptsDbExists;
        }

        public BackupFile(string name, string fullName, long length, string extension, DateTime creationTime, bool eventsDatabaseExists, bool voicesDatabaseExists, bool scriptsFolderExists, string sdk) : this(name, fullName, length, extension, creationTime, eventsDatabaseExists, voicesDatabaseExists, scriptsFolderExists)
        {
            Sdk = sdk;
        }

        /// <summary>
        /// 
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 
        /// </summary>
        public string FullName { get; }

        /// <summary>
        /// 
        /// </summary>
        public long Length { get; }

        /// <summary>
        /// 
        /// </summary>
        public string Extension { get; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime CreationTime { get; }

        /// <summary>
        /// 
        /// </summary>
        public bool EventsDatabaseExists { get; }

        /// <summary>
        /// 
        /// </summary>
        public bool VoicesDatabaseExists { get; }

        /// <summary>
        /// 
        /// </summary>
        public bool ScriptsFolderExists { get; }

        /// <summary>
        /// 
        /// </summary>
        public string Sdk { get; }
    }
}
