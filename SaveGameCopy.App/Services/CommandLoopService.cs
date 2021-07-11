using Microsoft.Extensions.Hosting;
using SaveGameCopy.App.Constants;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SaveGameCopy.App.Services
{
    internal class CommandLoopService : IHostedService
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly FileCopyService _fileCopyService;
        private bool _quit = false;

        public CommandLoopService(IHostApplicationLifetime hostApplicationLifetime, FileCopyService fileCopyService)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
            _fileCopyService = fileCopyService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                await CommandProcessingLoop(cancellationToken);
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred:\n{e.Message}");
            }
            finally
            {
                _hostApplicationLifetime.StopApplication();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async Task CommandProcessingLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && !_quit)
            {
                Console.WriteLine("\n...");

                var command = ReadCommand();
                switch (command)
                {
                    case Command.BackupSave:
                        await Save();
                        break;
                    case Command.RestoreSave:
                        await Restore();
                        break;
                    case Command.Exit:
                        _quit = true;
                        break;
                    case Command.ClearScreen:
                        Console.Clear();
                        break;
                    default:
                        throw new Exception("Unhandled Command returned from ReadCommand");
                }
            }
        }

        private async Task Save()
        {
            var result = await _fileCopyService.BackupSaveFileDirectory();
            switch (result)
            {
                case FileCopyResult.NoFiles:
                    Console.WriteLine("No directory in copy from");
                    break;
                case FileCopyResult.Ok:
                    Console.WriteLine("Ok!");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(result));
            }
        }

        private async Task Restore()
        {
            var result = await _fileCopyService.RestoreLatestBackup();
            switch (result)
            {
                case FileCopyResult.NoFiles:
                    Console.WriteLine("No backups to restore");
                    break;
                case FileCopyResult.Ok:
                    Console.WriteLine("Restored!");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(result));
            }
        }

        private static Command ReadCommand()
        {
            while (true)
            {
                var input = Console.ReadLine();
                switch (input?.ToLowerInvariant())
                {
                    case CommandConstants.Exit:
                    case CommandConstants.Quit:
                    case null: // E.g. CTRL+C/CTRL+Z
                        return Command.Exit;

                    case CommandConstants.Save:
                    case CommandConstants.Backup:
                        return Command.BackupSave;

                    case CommandConstants.Restore:
                        return Command.RestoreSave;

                    case CommandConstants.Clear:
                    case CommandConstants.ClearScreen:
                        return Command.ClearScreen;

                    default:
                        Console.WriteLine("What?");
                        break;
                }
            }
        }
    }
}
