namespace Application.Common.Exceptions;

/// <summary>
/// Thrown when a request violates a business rule (not a validation error).
/// Caught by GlobalExceptionHandlerMiddleware and returned as HTTP 400.
/// Examples: booking a past tour schedule, not enough available slots.
/// </summary>
public sealed class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message) { }
}
