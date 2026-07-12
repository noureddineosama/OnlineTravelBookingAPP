using Application;
using Infrastructure;
using Microsoft.OpenApi.Models;
using OnlineTravelBooking.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ── Clean Architecture DI ────────────────────────────────────
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ── CORS ──────────────────────────────────────────────────────
builder.Services.AddCors(options =>
    options.AddPolicy("AllowAll", p =>
        p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

// ── Controllers ───────────────────────────────────────────────
// JsonStringEnumConverter ensures all enums (e.g. FavoriteCategory) are
// serialized as their string names ("Tour", "Hotel", "Flight", "Car")
// rather than integer values. This gives the frontend a stable, readable
// contract it can use for routing (e.g. /tours/{id}) and conditional logic.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });


// ── Swagger ───────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title   = "Online Travel Booking API",
        Version = "v1",
        Description = "Clean Architecture — Domain / Application / Infrastructure / API"
    });

    // Display enum values as strings in Swagger UI (matches JsonStringEnumConverter)
    options.UseInlineDefinitionsForEnums();
});

var app = builder.Build();

// ── Middleware Pipeline ───────────────────────────────────────
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Online Travel Booking API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.MapControllers();

app.Run();
