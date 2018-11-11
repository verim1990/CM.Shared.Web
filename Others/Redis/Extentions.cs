using CM.Shared.Kernel.Others.Redis;
using Microsoft.Extensions.DependencyInjection;

namespace CM.Shared.Web.Others.Redis
{
    public static class ExtentionsForRedis
    {
        public static IServiceCollection IncludeRedis(this IServiceCollection services, RedisSettings redisSettings)
        {
            return services
                .AddDistributedRedisCache(options =>
                {
                    options.InstanceName = redisSettings.Host;
                    options.Configuration = redisSettings.Host;
                })
                .AddScoped(provider => redisSettings)
                .AddSingleton<RedisConnector>();
        }
    }
}
