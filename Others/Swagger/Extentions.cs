using CM.Shared.Kernel.Others.Swagger;
using CM.Shared.Kernel.Others.Swagger.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;

namespace CM.Shared.Web.Others.Swagger
{
    public static class ExtentionsForSwagger
    {
        public static IServiceCollection IncludeSwagger(this IServiceCollection services, SwaggerSettings swaggerSettings, string identityUrl)
        {
            return services
                .AddScoped(provider => swaggerSettings)
                .AddSwaggerGen(options =>
                {
                    options.DescribeAllEnumsAsStrings();

                    options.SwaggerDoc(swaggerSettings.Version, new Info
                    {
                        Title = swaggerSettings.Name,
                        Version = swaggerSettings.Version,
                        Description = swaggerSettings.Description,
                        TermsOfService = swaggerSettings.TermsOfService
                    });

                    options.AddSecurityDefinition("oauth2", new OAuth2Scheme
                    {
                        Type = "oauth2",
                        Flow = "implicit",
                        AuthorizationUrl = $"{identityUrl}{swaggerSettings.Endpoints.Authorize}",
                        TokenUrl = $"{identityUrl}{swaggerSettings.Endpoints.Token}",
                        Scopes = swaggerSettings.Scopes
                    });

                    options.CustomSchemaIds(x => x.FullName);
                    options.OperationFilter<AuthorizeCheckOperationFilter>();
                });
        }

        public static IApplicationBuilder UseSwaggerForCM(this IApplicationBuilder app, SwaggerSettings swaggerSettings)
        {
            return app.UseSwagger()
                .UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint(swaggerSettings.Endpoints.Manifest, swaggerSettings.FullName);
                    c.OAuthClientId(swaggerSettings.Client.Id);
                    c.OAuthClientSecret(swaggerSettings.Client.Secret);
                    c.OAuthAppName(swaggerSettings.FullName);
                });
        }
    }
}
