using CM.Shared.Kernel.Application.Settings;
using CM.Shared.Kernel.Others.Kong;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using System;

namespace CM.Shared.Web.Others.Kong
{
    public static class ExtentionsForKong
    {
        public static IServiceCollection IncludeKong(this IServiceCollection services, KongSettings kongSettings, KongServiceSettings kongServiceSettings, ServiceSettings serviceSettings)
        {
            return services
                .AddHttpClient()
                .AddScoped(provider => kongSettings)
                .AddScoped(provider => kongServiceSettings)
                .AddSingleton<KongConnector>();
        }

        public static IWebHost RegisterKong(this IWebHost webHost)
        {
            using (var scope = webHost.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<IWebHost>>();
                var kongConnector = services.GetService<KongConnector>();

                try
                {
                    logger.LogInformation($"Waiting for Kong");

                    var retry = Policy.HandleResult<bool>(isRunning => !isRunning)
                        .WaitAndRetryAsync(new[]
                        {
                            TimeSpan.FromSeconds(5),
                            TimeSpan.FromSeconds(10),
                            TimeSpan.FromSeconds(15)
                        })
                        .ExecuteAsync(async () =>
                        {
                            await kongConnector.IsRunning();
                            await kongConnector.Register();

                            return true;
                        });

                    logger.LogInformation($"Connection to Kong established");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Coudn't established connection to Kong");

                    throw ex;
                }
            }

            return webHost;
        }
    }
}
