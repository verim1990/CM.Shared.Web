using CM.Shared.Kernel.Others.Swagger;
using CM.Shared.Web.Middlewares;
using Microsoft.AspNetCore.Builder;

namespace CM.Shared.Web
{
    public static class ApplicationBuilderExtentions
    {
        public static IApplicationBuilder UseCorsForCM(this IApplicationBuilder app)
        {
            return app.UseCors("default");
        }

        public static IApplicationBuilder UseAuthenticationForCM(this IApplicationBuilder app)
        {
            return app.UseAuthentication();
        }

        public static IApplicationBuilder UseResponseWrapperForCM(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ResponseWrapper>();
        }
    }
}
