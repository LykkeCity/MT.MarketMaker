using Microsoft.AspNetCore.Mvc.Controllers;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MarginTrading.MarketMaker.Infrastructure.Implementation
{
    public class CustomOperationIdOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            var actionDescriptor = (ControllerActionDescriptor) context.ApiDescription.ActionDescriptor;
            operation.OperationId = actionDescriptor.ControllerName + actionDescriptor.ActionName;
        }
    }
}