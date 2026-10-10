using System.Text.Json.Nodes;
using Microsoft.OpenApi;
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
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (!context.Type.IsEnum) return;

        if (schema is OpenApiSchema openApiSchema)
        {
            openApiSchema.Enum = Enum.GetNames(context.Type)
                                    .Select(name => (JsonNode)JsonValue.Create(name)!)
                                    .ToList();
            openApiSchema.Type = JsonSchemaType.String;
            openApiSchema.Format = null;
        }
    }
}
