using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SaveGameCopy.App.Services;
using System;
using System.Threading.Tasks;

namespace SaveGameCopy.App
{
    class Program
    {
        static async Task<int> Main()
        {
            var host = CreateHostBuilder();
            await host.RunConsoleAsync();

            return Environment.ExitCode;
        }

        private static IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHostedService<CommandLoopService>();

                    services.AddSingleton<FileCopyService>();
                });
        }
    }
}
