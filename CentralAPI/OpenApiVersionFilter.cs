using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class OpenApiVersionFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        // For Swashbuckle.AspNetCore 6.0+ (which you're using)
        // The OpenAPI version is automatically set to 3.0.1
        // No need to manually set it in newer versions
    }
}