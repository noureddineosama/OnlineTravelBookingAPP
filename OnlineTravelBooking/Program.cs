using Application;
using Application.Common.Interfaces;
using Infrastructure;
using Infrastructure.Services;
using Microsoft.OpenApi.Models;
using OnlineTravelBooking.Middleware;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ── Clean Architecture DI ────────────────────────────────────
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ── JWT Authentication ────────────────────────────────────────────────────────
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>();
if (jwtSettings is null)
    throw new InvalidOperationException("JWT Settings are not configured in appsettings.json.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JWtBearer.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = jwtSettings.Issuer,
        ValidAudience            = jwtSettings.Audience,
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKEY))
    };
});

builder.Services.AddAuthorization();

// ── CORS ──────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
    options.AddPolicy("AllowAll", p =>
        p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

// ── Controllers ───────────────────────────────────────────────
builder.Services.AddControllers();

//. Configuration
builder.Services.AddApplication();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddTransient<ICurrentIUserService, CurrentUserService>();

builder.Services.AddHttpContextAccessor();
//builder.Services.AddSwaggerGen(options => 
//            options.UseInlineDefinitionsForEnums());
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
}); //. These lines aim to convert the numbers for the enums to string (in process of entering the data)

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
});

var app = builder.Build();

// ── Middleware Pipeline ───────────────────────────────────────
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseMiddleware<MyCustomGlobalExceptionHandlerMiddleware>();
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

app.MapControllers();

app.Run();
