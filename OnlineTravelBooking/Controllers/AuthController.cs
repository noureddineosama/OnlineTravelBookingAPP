using Application.Features.Auth.Commands.Login;
using Application.Features.Auth.Commands.Logout;
using Application.Features.Auth.Commands.Register;
using Application.Features.Auth.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace OnlineTravelBooking.Controllers;

[Route("api/[controller]")]
[ApiController]
[EnableRateLimiting("auth-fixed-window")]
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
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _mediator.Send(new Application.Features.Auth.Commands.RefreshToken.RefreshTokenCommand(request.RefreshToken));

        if (!result.Success)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        var result = await _mediator.Send(new Application.Features.Auth.Commands.Logout.LogoutCommand(request.RefreshToken));

        if (!result.Success)
        {
            return BadRequest(result);
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

public sealed record RefreshTokenRequest(
    string RefreshToken
);

public sealed record LogoutRequest(
    string RefreshToken
);
