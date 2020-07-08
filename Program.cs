using System;
using System.Runtime.InteropServices;
using Gelf.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NotificationServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
                .ConfigureLogging(ConfigureLogging)
                .UseWindowsService();
        }

        private static void ConfigureLogging(HostBuilderContext context, ILoggingBuilder builder)
        {
            var configuration = context.Configuration;

            // <see cref="https://github.com/mattwcole/gelf-extensions-logging"/>
            // Read GelfLoggerOptions from appsettings.json
            var session = configuration.GetSection("Graylog");
            builder.Services.Configure<GelfLoggerOptions>(session);

            // Optionally configure GelfLoggerOptions further.
            builder.Services.PostConfigure<GelfLoggerOptions>(options =>
            {
                options.AdditionalFields["machine-name"] = Environment.MachineName;
                options.AdditionalFields["operation-system"] = RuntimeInformation.OSDescription;
            });

            // Read Logging settings from appsettings.json and add providers.
            builder.AddConfiguration(configuration.GetSection("Logging"))
                .AddGelf();
        }

        #region MyRegion

        //private static void Options(KestrelServerOptions o)
        //{
        //    o.Limits.MaxConcurrentConnections = null;
        //    o.Limits.MaxConcurrentUpgradedConnections = null;
        //    o.Limits.MaxRequestBufferSize = null;
        //    o.Limits.MaxRequestHeaderCount = 4096;
        //    o.Limits.MaxRequestHeadersTotalSize = 32768 * o.Limits.MaxRequestHeaderCount;
        //    // <see cref="https://stackoverflow.com/a/47112438"/>
        //    o.Limits.MaxRequestBodySize = null;
        //    // <see cref="https://stackoverflow.com/a/47809150"/>
        //    // o.Limits.KeepAliveTimeout = TimeSpan.FromHours(1);
        //    // o.Limits.RequestHeadersTimeout = TimeSpan.FromHours(1);
        //    o.Limits.MaxResponseBufferSize = null;
        //    o.AddServerHeader = false;
        //}

        #endregion
    }
}