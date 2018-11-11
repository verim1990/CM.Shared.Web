using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace CM.Shared.Web.Others.FluentValidation
{
    public static class ExtentionsForFluentValidation
    {
        public static IMvcBuilder IncludeFluentValidation(this IMvcBuilder mvc, Type handlerType = null)
        {
            return mvc.AddFluentValidation(fv =>
            {
                if (handlerType != null)
                {
                    fv.RegisterValidatorsFromAssemblyContaining(handlerType);
                }
            });
        }
    }
}
