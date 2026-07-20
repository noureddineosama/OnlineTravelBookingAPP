using Amazon;
using Amazon.S3;
using Application;
using Application.Common.Interfaces;
using Infrastructure;
using Infrastructure.AWSSettings;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OnlineTravelBooking.Middleware;
using OnlineTravelBooking.Swagger;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ── Clean Architecture DI ─────────────────────────────────────────────────────
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

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
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// ── CORS ──────────────────────────────────────────────────────
builder.Services.AddCors(options =>
    options.AddPolicy("AllowAll", p =>
        p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

//_______________________________________
builder.Services.AddAuthentication(options => { 
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; 
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; })
    .AddJwtBearer(options => { 
        options.TokenValidationParameters = new TokenValidationParameters 
        { 
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true, 
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"], IssuerSigningKey = 
    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKEY"]!)) 
        }; 
    }); 
builder.Services.AddAuthorization();

// ── Controllers ───────────────────────────────────────────────────────────────
// JsonStringEnumConverter ensures all enums (e.g. FavoriteCategory) are
// serialized as their string names ("Tour", "Hotel", "Flight", "Car")
// rather than integer values — gives the frontend a stable, readable contract.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddTransient<ICurrentIUserService, CurrentUserService>();
builder.Services.AddHttpContextAccessor();
//builder.Services.AddSwaggerGen(options => 
//            options.UseInlineDefinitionsForEnums());

// ── Swagger ───────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "Online Travel Booking API",
        Version     = "v1",
        Description = "Clean Architecture — Domain / Application / Infrastructure / API"
    });

    // Enum values appear as strings ("Tour", "Hotel") instead of integers.
    options.SchemaFilter<StringEnumSchemaFilter>();

    // Surface all XML <summary> comments as Swagger descriptions.
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);

    // JWT Bearer auth in Swagger UI
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.ApiKey,
        Scheme       = "Bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Enter 'Bearer' [space] and then your token.\r\n\r\nExample: \"Bearer eyJhbGci...\""
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



var app = builder.Build();

// ── Middleware Pipeline ───────────────────────────────────────────────────────
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseMiddleware<MeasuringExecutingTimeMiddleware>();

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
app.UseAuthorization();

app.MapControllers();

app.Run();
