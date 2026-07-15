namespace Application.Common.Exceptions;

/// <summary>
/// Thrown when an operation conflicts with the current state of a resource.
/// Caught by GlobalExceptionHandlerMiddleware and returned as HTTP 409.
/// Examples: duplicate email, item already in favourites, booking already cancelled.
/// </summary>
public sealed class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}
