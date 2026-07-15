using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OnlineTravelBooking.Swagger;

/// <summary>
/// Ensures all enum types are described as strings ("Tour", "Hotel", "Flight", "Car")
/// in Swagger rather than integers. This matches the JsonStringEnumConverter
/// registered in Program.cs, so the Swagger UI contract matches the real API contract.
/// Applies to both request body schemas and query parameter dropdowns.
/// </summary>
public sealed class StringEnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (!context.Type.IsEnum) return;

        schema.Enum   = Enum.GetNames(context.Type)
                            .Select(name => (IOpenApiAny)new OpenApiString(name))
                            .ToList();
        schema.Type   = "string";
        schema.Format = null;
    }
}
