using CM.Shared.Kernel.Others.MediatR.Behaviours;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace CM.Shared.Web.Others.MediatR
{
    public static class ExtentionsForMediatR
    {
        public static IServiceCollection IncludeMediator(this IServiceCollection services, params Type[] handlerTypes)
        {
            return services
                .AddMediatR()
                .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidatorBehavior<,>));
        }
    }
}
