using System.Net;
using System.Text.Json;
using Application.Common.Models;
using Sentry;
using ValidationException  = Application.Common.Exceptions.ValidationException;
using NotFoundException    = Application.Common.Exceptions.NotFoundException;
using ConflictException    = Application.Common.Exceptions.ConflictException;
using BadRequestException  = Application.Common.Exceptions.BadRequestException;

namespace OnlineTravelBooking.Middleware;

public sealed class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, response) = exception switch
        {
            ValidationException ve => (
                HttpStatusCode.BadRequest,
                ApiResponse<object>.Fail("Validation failed.", (int)HttpStatusCode.BadRequest, ve.Errors.ToList())
            ),

            NotFoundException ne => (
                HttpStatusCode.NotFound,
                ApiResponse<object>.Fail(ne.Message, (int)HttpStatusCode.NotFound)
            ),

            ConflictException ce => (
                HttpStatusCode.Conflict,
                ApiResponse<object>.Fail(ce.Message, (int)HttpStatusCode.Conflict)
            ),

            BadRequestException be => (
                HttpStatusCode.BadRequest,
                ApiResponse<object>.Fail(be.Message, (int)HttpStatusCode.BadRequest)
            ),

            _ => (
                HttpStatusCode.InternalServerError,
                ApiResponse<object>.Fail("An unexpected error occurred.", (int)HttpStatusCode.InternalServerError)
            )
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
            SentrySdk.AddBreadcrumb(
                message: $"{context.Request.Method} {context.Request.Path}",
                category: "http.request",
                level: BreadcrumbLevel.Error,
                data: new Dictionary<string, string>
                {
                    ["query"]  = context.Request.QueryString.ToString(),
                    ["status"] = ((int)statusCode).ToString()
                }
            );
            SentrySdk.CaptureException(exception);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}
