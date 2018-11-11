using CM.Shared.Kernel.Application.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;

namespace CM.Shared.Web.Filters
{
    public class ValidateModelStateFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ModelState.IsValid)
            {
                return;
            }

            var validationErrors = context.ModelState.ToDictionary(kvp => kvp.Key,
                    kvp => kvp.Value
                        .Errors
                        .Select(e => string.IsNullOrEmpty(e.ErrorMessage) ? e.Exception.Message : e.ErrorMessage)
                        .ToArray())
                .Where(m => m.Value.Any())
                .ToList();

            var json = new ValidationResponse(validationErrors);

            context.Result = new BadRequestObjectResult(json);
        }
    }
}
