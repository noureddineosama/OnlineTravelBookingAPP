using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Auth.DTOs
{
    public sealed record LogoutRequest(
    string RefreshToken
);
}
