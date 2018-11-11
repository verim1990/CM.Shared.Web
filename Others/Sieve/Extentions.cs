using Microsoft.Extensions.DependencyInjection;
using Sieve.Services;

namespace CM.Shared.Web.Others.Sieve
{
    public static class ExtentionsForSieve
    {

        public static IServiceCollection IncludeSieve(this IServiceCollection services)
        {
            return services.AddScoped<SieveProcessor>();
        }
    }
}
