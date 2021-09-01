using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SbisParser.Models;
using SbisParser.Interfaces;
namespace SbisParser
{
    class Program
    {
        static async Task Main(string[] args) =>
            await CreateHostbuilding(args).Build().RunAsync();

        static IHostBuilder CreateHostbuilding(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureHostConfiguration(conf =>
            {
                conf.SetBasePath(Directory.GetCurrentDirectory());
                conf.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                conf.AddCommandLine(args);
            })
            .ConfigureServices((context, services) =>
            {
                var configRoot = context.Configuration;
                services.Configure<SbisSettings>(configRoot.GetSection(nameof(SbisSettings)));
                services.Configure<DataBaseSettings>(configRoot.GetSection(nameof(DataBaseSettings)));
                services.AddHostedService<ParserService>();
                services.AddScoped<IBaseService, BaseService>();
            })
            .UseConsoleLifetime();
    }
}
