using AppCheck.Helper.Attributes;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AppCheck.Helper.Header
{
    public class CustomHeader : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var hasExcludeCustomHeaderAttribute = context.MethodInfo
           .GetCustomAttributes(true)
           .OfType<ExcludeCustomHeaderAttribute>()
           .Any();

            if (hasExcludeCustomHeaderAttribute)
            {
                return;
            }

            if (operation.Parameters == null)
                operation.Parameters = new List<OpenApiParameter>();

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-Firebase-AppCheck",
                In = ParameterLocation.Header,
                Required = true,
                Schema = new OpenApiSchema
                {
                    Type = "string"
                }
            });
        }
    }
}
