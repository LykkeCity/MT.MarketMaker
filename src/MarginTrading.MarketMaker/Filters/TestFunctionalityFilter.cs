using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MarginTrading.MarketMaker.Filters
{
    public class TestFunctionalityFilter : ActionFilterAttribute
    {
        public static bool TestsEnabled { get; } =
            Environment.GetEnvironmentVariable("ENV_INFO") == "DEVELOPMENT_ENVIRONMENT";

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            ValidateTestsEnabled();
            await base.OnActionExecutionAsync(context, next);
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            ValidateTestsEnabled();
            base.OnActionExecuting(context);
        }

        public static void ValidateTestsEnabled()
        {
            if (!TestsEnabled)
            {
                throw new Exception("Using testing functionality is restricted on production environment");
            }
        }
    }
}