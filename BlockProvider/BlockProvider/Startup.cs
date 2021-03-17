using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BlockProvider
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();

            var rqmConnection = Environment.GetEnvironmentVariable("RMQ_CONNECTION");
            var blockCallIntervalStr = Environment.GetEnvironmentVariable("BLOCK_CALL_INTERVAL");

            if (string.IsNullOrWhiteSpace(rqmConnection) || string.IsNullOrWhiteSpace(blockCallIntervalStr))
            {
                Console.WriteLine("RMQ_CONNECTION and BLOCK_CALL_INTERVAL required");
                throw new ArgumentException("RMQ_CONNECTION and BLOCK_CALL_INTERVAL required");
            }
            
            var blockCallInterval = int.Parse(blockCallIntervalStr);
            
            services.AddHostedService(x =>
                new BlockScheduleService(x.GetRequiredService<ILogger<BlockScheduleService>>(),rqmConnection,TimeSpan.FromSeconds(blockCallInterval)));
            
            services.AddControllers()
                .AddNewtonsoftJson(options => options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore);

            services
                .AddRouting(options => options.LowercaseUrls = true)
                .AddMvcCore()
                .AddApiExplorer();

            services
                .AddCors(options =>
                {
                    options.AddPolicy("AllowAll", p => p
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowAnyOrigin()
                    );
                });
        }

        public void Configure(IApplicationBuilder app, IHostApplicationLifetime applicationLifetime)
        {
            app.UseRouting()
                .UseCors("AllowAll")
                .UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}