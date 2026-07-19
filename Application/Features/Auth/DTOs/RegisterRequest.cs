using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Auth.DTOs
{
    public sealed record RegisterRequest(
     string Name,
     string Email,
     string Password,
     string? Phone = null,
     int RoleId = 1
 );
}
