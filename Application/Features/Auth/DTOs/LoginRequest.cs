using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Auth.DTOs
{
    public sealed record LoginRequest(
    string Email,
    string Password
);
}
