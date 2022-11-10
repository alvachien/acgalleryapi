using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace GalleryAPI
{
    public class Program
    {
        public static int Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
#if DEBUG
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Information)
#else
            .MinimumLevel.Warning()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
#endif
            .Enrich.FromLogContext()
#if DEBUG
            .WriteTo.Console(theme: SystemConsoleTheme.Colored)
#else
            .WriteTo.File(
                    @"C:\WebApps\Logs\ACGalleryAPI\log.txt",
                    fileSizeLimitBytes: 1_000_000,
                    rollOnFileSizeLimit: true,
                    shared: true,
                    flushToDiskInterval: TimeSpan.FromSeconds(30))
#endif
            .CreateLogger();

            try
            {
                Log.Information("Starting web host");
                CreateHostBuilder(args).Build().Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
