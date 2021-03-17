using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BlockLogger
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();

            var rmqConnection = Environment.GetEnvironmentVariable("RMQ_CONNECTION");
            var pgConnection = Environment.GetEnvironmentVariable("PG_CONNECTION");
            var redisConnection = Environment.GetEnvironmentVariable("REDIS_CONNECTION");

            if (string.IsNullOrWhiteSpace(rmqConnection) || string.IsNullOrWhiteSpace(pgConnection) || string.IsNullOrWhiteSpace(redisConnection))
            {
                Console.WriteLine("RMQ_CONNECTION and PG_CONNECTION and REDIS_CONNECTION required");
                throw new ArgumentException("RMQ_CONNECTION and BLOCK_CALL_INTERVAL and REDIS_CONNECTION required");
            }

            services.AddHostedService(x => new BlockListener(
                x.GetRequiredService<ILogger<BlockListener>>(),
                rmqConnection,
                pgConnection,
                redisConnection));
            
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