using Autofac;
using Autofac.Extensions.DependencyInjection;
using CM.Shared.Kernel.Application.Bus;
using CM.Shared.Kernel.Application.Settings;
using CM.Shared.Web.Others.MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace CM.Shared.Web
{
    public static class ServicesExtentions
    {
        public static IServiceCollection Initialize<T>(this IServiceCollection services, IConfiguration configuration, ServiceSettings serviceSettings) where T: class
        {
            return services
                .Configure<T>(configuration)
                .AddScoped(provider => serviceSettings);
        }

        public static IServiceCollection IncludeBus(this IServiceCollection services, params Type[] handlerTypes)
        {
            return services
                .IncludeMediator(handlerTypes)
                .AddScoped<IBus, Bus>();
        }

        public static IServiceCollection IncludeCors(this IServiceCollection services, params string[] origins)
        {
            return services
                .AddCors(options =>
                {
                    options.AddPolicy("default",
                        builder => builder
                            .WithOrigins(origins)
                            .AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials());
                });
        }

        public static IServiceCollection IncludeAuthentitcationForSpa(this IServiceCollection services, string authorityUrl)
        {
            services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddCookie()
                .AddOpenIdConnect(options =>
                {
                    options.Authority = authorityUrl;
                    options.RequireHttpsMetadata = false;

                    options.ClientId = "js";
                    options.ClientSecret = "secret";

                    options.ResponseType = "code id_token";
                    options.Scope.Add("wallet");
                    options.Scope.Add("exchange");

                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.SaveTokens = true;
                });

            return services;
        }

        public static IServiceCollection IncludeAuthenticationForIdentity(this IServiceCollection services)
        {
            services
                .AddAuthentication()
                .AddFacebook(o =>
                {
                    o.AppId = "asdasdad";
                    o.AppSecret = "asdasdad";
                })
                .AddGoogle(o =>
                {
                    o.ClientId = "adasdasda";
                    o.ClientSecret = "asdasdasdas";
                })
                .AddTwitter(o =>
                {
                    o.ConsumerKey = "asdasdA";
                    o.ConsumerSecret = "asdsadsada";
                });

            return services;
        }

        public static IServiceCollection IncludeAuthenticationForAPI(this IServiceCollection services, string audianceName, string authorityUrl)
        {
            services
                .AddAuthentication(o =>
                {
                    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(cfg =>
                {
                    cfg.Audience = audianceName.ToLower();
                    cfg.Authority = $"http://{authorityUrl}";
                    cfg.RequireHttpsMetadata = false;
                });

            return services;
        }

        public static AutofacServiceProvider RegisterModules(this IServiceCollection services, params Module[] modules)
        {
            var container = new ContainerBuilder();
            container.Populate(services);

            foreach (Module module in modules)
            {
                container.RegisterModule(module);
            }

            return new AutofacServiceProvider(container.Build());
        }
    }
}
