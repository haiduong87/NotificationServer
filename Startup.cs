using System;
using System.Threading.Channels;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotificationServer.Dto;
using NotificationServer.HostedServices;
using NotificationServer.Miscellaneous;

namespace NotificationServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<KestrelServerOptions>(Configuration.GetSection("Kestrel"));

            services.AddControllers();

            services.AddSingleton<Configuration>();
            services.AddSingleton<NatsConnectionPool>();
            services.AddSingleton(s => Channel.CreateBounded<DatabaseNotification>(new BoundedChannelOptions(1000000)
            {
                FullMode = BoundedChannelFullMode.DropOldest
            }));

            services.AddHostedService<NatsWorker>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger,
            Configuration configuration)
        {
            var now = DateTimeOffset.Now;
            logger.LogInformation($"Startup.Configure: {now:O}");
            configuration.LogObject.StartTime = now;

            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}