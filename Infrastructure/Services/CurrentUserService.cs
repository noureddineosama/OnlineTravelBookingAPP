using Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.Services
{
    public  class CurrentUserService : ICurrentIUserService 
    {
        private readonly IHttpContextAccessor contextAccessor;

        public CurrentUserService(IHttpContextAccessor  contextAccessor)
        {
            this.contextAccessor = contextAccessor;
        }
        //.we will need to user to be logged in to use this interface or service 

        public long UserId => long.Parse(contextAccessor.HttpContext!.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        public string Email => contextAccessor.HttpContext!.User.FindFirst(ClaimTypes.Email)!.Value;
    }
}
