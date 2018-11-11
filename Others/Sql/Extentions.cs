using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace CM.Shared.Kernel.Others.Sql
{
    public static class ExtentionsForSql
    {
        public static IServiceCollection IncludeSqlServer<T>(this IServiceCollection services, SqlSettings sqlSettings, SqlContextSettings sqlContextSettings, bool shouldAddEntityFrameworkSqlServer = false) where T: DbContext
        {
            sqlSettings.AddContextSettings(sqlContextSettings);

            services
                .AddScoped(provider => sqlSettings)
                .AddDbContext<T>(options => options.UseSqlServer(sqlSettings.ConnectionString, GetSqlOptionBuilder<T>()));

            if (shouldAddEntityFrameworkSqlServer)
                services.AddEntityFrameworkSqlServer();

            return services;
        }

        public static IWebHost RegisterSql<TContext>(this IWebHost webHost, Func<TContext, IServiceProvider, Task> seeder = null) where TContext : DbContext
        {
            return webHost
                .ExecuteForSql<TContext>("Migration", async (context, services) => await context.Database.MigrateAsync())
                .ExecuteForSql<TContext>("Seeding", async (context, services) =>
                {
                    if (seeder != null)
                    {
                        await seeder(context, services);
                    }
                });
        }

        private static IWebHost ExecuteForSql<TContext>(this IWebHost webHost, string taskName, Func<TContext, IServiceProvider, Task> task = null) where TContext : DbContext
        {
            using (var scope = webHost.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetService<TContext>();
                var logger = services.GetRequiredService<ILogger<TContext>>();

                try
                {
                    logger.LogInformation($"Sql Server: {taskName} starting.");

                    Policy
                        .Handle<SqlException>()
                        .WaitAndRetryAsync(new[]
                        {
                            TimeSpan.FromSeconds(5),
                            //TimeSpan.FromSeconds(10),
                            //TimeSpan.FromSeconds(15)
                        })
                        .ExecuteAsync(async () => {
                            logger.LogInformation($"Sql Server: Attempt for {taskName}.");

                            await task(context, services);
                        }).Wait();

                    logger.LogInformation($"Sql Server: {taskName} finished.");
                }
                catch (Exception ex)
                {
                    logger.LogError($"Sql Server: {taskName} failed.");

                    throw ex;
                }
            }

            return webHost;
        }

        public static Action<SqlServerDbContextOptionsBuilder> GetSqlOptionBuilder<T>()
        {
            return new Action<SqlServerDbContextOptionsBuilder>(sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(T).Assembly.GetName().Name);
                sqlOptions.EnableRetryOnFailure(15, TimeSpan.FromSeconds(30), new List<int>());
            });
        }
    }
}
