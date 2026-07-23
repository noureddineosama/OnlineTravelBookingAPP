// ════════════════════════════════════════════════════════════════════════════
//  Online Travel Booking API  —  Composition Root
//  Clean Architecture: Domain → Application → Infrastructure → API
// ════════════════════════════════════════════════════════════════════════════

using Application;
using Application.Common.Interfaces;
using Infrastructure;
using Infrastructure.Services;
using Microsoft.OpenApi.Models;
using OnlineTravelBooking.Middleware;
using OnlineTravelBooking.Swagger;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.Extensions.Caching.Hybrid;
using System.Security.Claims;
using Sentry.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ── 1. Clean Architecture layers ─────────────────────────────────────────────
//  AddApplication  : MediatR, FluentValidation, AutoMapper, pipeline behaviors
//  AddInfrastructure: EF Core, JWT, caching, rate limiting, Stripe, AWS, repositories
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ── 2. CORS ───────────────────────────────────────────────────────────────────
//.----Fixed Rate Limiting Registeration 
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("auth-fixed-window", context =>
    {
        var key = context.Connection.RemoteIpAddress?.ToString()
                  ?? "Anonymous";

        return RateLimitPartition.GetFixedWindowLimiter(key,
           _ => new FixedWindowRateLimiterOptions
           {
               PermitLimit = 5,
               AutoReplenishment = true,
               QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
               Window = TimeSpan.FromSeconds(10),
               QueueLimit = 0 //. don't put anything in queue and return to the customer 429 response
           });
    });

    options.AddPolicy("flight-read", context =>
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var key = !string.IsNullOrWhiteSpace(userId)
            ? $"user:{userId}"
            : $"ip:{context.Connection.RemoteIpAddress?.ToString() ?? "anonymous"}";

        return RateLimitPartition.GetFixedWindowLimiter(
            key,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 60,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                AutoReplenishment = true
            });
    });

    options.AddPolicy("flight-write", context =>
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var key = !string.IsNullOrWhiteSpace(userId)
            ? $"user:{userId}"
            : $"ip:{context.Connection.RemoteIpAddress?.ToString() ?? "anonymous"}";

        return RateLimitPartition.GetFixedWindowLimiter(
            key,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                AutoReplenishment = true
            });
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// ── CORS ──────────────────────────────────────────────────────
builder.Services.AddCors(options =>
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()));

// ── 3. Authorization (JWT Bearer registered inside AddInfrastructure) ─────────
builder.Services.AddAuthorization();

// ── 4. Controllers ────────────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
        opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<ICurrentIUserService, CurrentUserService>();

// ── 5. HybridCache ────────────────────────────────────────────────────────────
builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions = new Microsoft.Extensions.Caching.Hybrid.HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(30),
        LocalCacheExpiration = TimeSpan.FromMinutes(5)
    };
});
//______________Sentry____________________________________
// UseSentry() with no arguments reads ALL settings from the (Sentry) section
builder.WebHost.UseSentry();
builder.Services.Configure<SentryAspNetCoreOptions>(options =>
{
    options.Environment = builder.Environment.EnvironmentName;
    var version = System.Reflection.Assembly
        .GetExecutingAssembly()
        .GetName()
        .Version?.ToString() ?? "1.0.0";
    options.Release = $"online-travel-booking@{version}";

});

// ── 6. Swagger / OpenAPI ──────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "Online Travel Booking API",
        Version     = "v1",
        Description = "Clean Architecture — Domain · Application · Infrastructure · API"
    });

    options.SchemaFilter<StringEnumSchemaFilter>();

    var xmlPath = Path.Combine(
        AppContext.BaseDirectory,
        $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml");
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.ApiKey,
        Scheme       = "Bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Enter: Bearer {your-token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ════════════════════════════════════════════════════════════════════════════
//  Middleware pipeline
// ════════════════════════════════════════════════════════════════════════════

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseMiddleware<MeasuringExecutingTimeMiddleware>();

// ── Sentry performance tracing ────────────────────────────────────────────────
// Creates one Sentry "transaction" per HTTP request so you can see
// slow endpoints in the Performance tab of your Sentry dashboard.
app.UseSentryTracing();

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
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();
app.UseRateLimiter();   // after auth so identity is available to future user-scoped policies

app.MapControllers();

app.Run();
