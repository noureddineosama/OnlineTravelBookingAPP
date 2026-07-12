using Application.Features.Auth.Commands.Login;
using Application.Features.Auth.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace OnlineTravelBooking.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _mediator;

    public AuthController(ISender mediator) => _mediator = mediator;

    // OnlineTravelBooking\Controllers\AuthController.cs
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        
        var result = await _mediator.Send(new RegisterCommand(
            request.Name,
            request.Email,
            request.Password,
            request.Phone,
            request.RoleId
        ));

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Authenticate a user and receive a JWT token.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _mediator.Send(new LoginCommand(
            request.Email,
            request.Password
        ));

        if (!result.Success)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }
}

// ── Request DTOs ─────────────────────────────────────────────────────────────

public sealed record RegisterRequest(
    string Name,
    string Email,
    string Password,
    string? Phone = null,
    int RoleId = 1
);

public sealed record LoginRequest(
    string Email,
    string Password
);
