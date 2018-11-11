using CM.Shared.Kernel.Others.Postgres;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using Polly;
using System;
using System.Threading.Tasks;

namespace CM.Shared.Web.Others.Postgres
{
    public static class ExtentionsForPostgres
    {
        public static IServiceCollection IncludePostgres<T>(this IServiceCollection services, PostgresSettings postgresSettings, PostgresContextSettings postgresContextSettings, bool shouldAddEntityFrameworkNpgsql = false) where T : DbContext
        {
            postgresSettings.AddContextSettings(postgresContextSettings);

            services
                .AddScoped(provider => postgresSettings)
                .AddDbContext<T>(options => options.UseNpgsql(postgresSettings.ConnectionString));

            if (shouldAddEntityFrameworkNpgsql)
                services.AddEntityFrameworkNpgsql();

            return services;
        }

        public static IWebHost RegisterPostgres<TContext>(this IWebHost webHost, Func<TContext, IServiceProvider, Task> seeder = null) where TContext : DbContext
        {
            return webHost
                .ExecuteForPostgres<TContext>("Migration", async (context, services) =>  await context.Database.MigrateAsync())
                .ExecuteForPostgres<TContext>("Seeding", async (context, services) =>
                {
                    if (seeder != null)
                    {
                        await seeder(context, services);
                    }
                });
        }

        private static IWebHost ExecuteForPostgres<TContext>(this IWebHost webHost, string taskName, Func<TContext, IServiceProvider, Task> task = null) where TContext : DbContext
        {
            using (var scope = webHost.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetService<TContext>();
                var logger = services.GetRequiredService<ILogger<TContext>>();

                try
                {
                    logger.LogInformation($"Postgres: {taskName} starting.");

                    var retry = Policy
                        .Handle<PostgresException>()
                        .WaitAndRetry(new[]
                        {
                            TimeSpan.FromSeconds(5),
                            //TimeSpan.FromSeconds(10),
                            //TimeSpan.FromSeconds(15)
                        })
                        .ExecuteAsync(async () => {
                            logger.LogInformation($"Postgres: Attempt for {taskName}.");

                            await task(context, services);
                        });

                    logger.LogInformation($"Postgres: {taskName} finished.");
                }
                catch (Exception ex)
                {
                    logger.LogError($"Postgres: {taskName} failed.");

                    throw ex;
                }
            }

            return webHost;
        }
    }
}
