using Microsoft.Extensions.Configuration;
using SaveGameCopy.App.Constants;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SaveGameCopy.App.Services
{
    internal class FileCopyService
    {
        private readonly IConfiguration _configuration;

        public FileCopyService(IConfiguration configuration)
        {
            _configuration = configuration;
            VerifyConfiguration();
        }

        internal async Task<FileCopyResult> BackupSaveFileDirectory()
        {
            var copyFromDirectory = _configuration.GetValue<string>(AppsettingsKeys.LiveDirectory);
            if (!Directory.Exists(copyFromDirectory))
                return FileCopyResult.NoFiles;

            var copyToDirectory = _configuration.GetValue<string>(AppsettingsKeys.BackupDirectory);
            var copyToWithTimestamp = Path.Combine(copyToDirectory, $"backup.{DateTime.Now:yyyy-MM-dd.HHmmss}");

            CopyFilesRecursively(copyFromDirectory, copyToWithTimestamp, false);
            return FileCopyResult.Ok;
        }

        internal async Task<FileCopyResult> RestoreLatestBackup()
        {
            var copyFromDirectory = GetLatestBackupFromDirectory();
            var copyToDirectory = _configuration.GetValue<string>(AppsettingsKeys.LiveDirectory);

            if (copyFromDirectory == null)
                return FileCopyResult.NoFiles;

            CopyFilesRecursively(copyFromDirectory.FullName, copyToDirectory, true);
            return FileCopyResult.Ok;
        }

        private DirectoryInfo GetLatestBackupFromDirectory()
        {
            var backupDirectory = _configuration.GetValue<string>(AppsettingsKeys.BackupDirectory);
            return Directory.GetDirectories(backupDirectory)
                .Select(x => new DirectoryInfo(x))
                .OrderByDescending(x => x.CreationTimeUtc)
                .FirstOrDefault();
        }

        private void CopyFilesRecursively(string sourceDirectoryPath, string destinationDirectoryPath, bool allowOverwrite)
        {
            var directory = new DirectoryInfo(sourceDirectoryPath);

            if (!directory.Exists)
                throw new DirectoryNotFoundException($"Source directory does not exist or could not be found: {sourceDirectoryPath}");

            Directory.CreateDirectory(destinationDirectoryPath);

            foreach (FileInfo file in directory.GetFiles())
            {
                var copyToFilePath = Path.Combine(destinationDirectoryPath, file.Name);
                file.CopyTo(copyToFilePath, true);
            }

            foreach (DirectoryInfo subdirectory in directory.GetDirectories())
            {
                var destinationSubdirectoryPath = Path.Combine(destinationDirectoryPath, subdirectory.Name);
                CopyFilesRecursively(subdirectory.FullName, destinationSubdirectoryPath, allowOverwrite);
            }
        }

        private void VerifyConfiguration()
        {
            var liveDirectory = _configuration.GetValue<string>(AppsettingsKeys.LiveDirectory);
            var backupDirectory = _configuration.GetValue<string>(AppsettingsKeys.BackupDirectory);

            if (!Directory.Exists(liveDirectory))
                throw new InvalidOperationException($"Configured live directory does not exist:\n{liveDirectory}");

            if (!Directory.Exists(backupDirectory))
                throw new InvalidOperationException($"Configured backup directory does not exist:\n{backupDirectory}");
        }
    }
}
