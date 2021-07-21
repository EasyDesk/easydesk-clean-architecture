using EasyDesk.CleanArchitecture.Application.UserInfo;
using EasyDesk.CleanArchitecture.Domain.Model.Roles;
using EasyDesk.CleanArchitecture.Infrastructure.Jwt;
using EasyDesk.Tools.Options;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace EasyDesk.CleanArchitecture.Infrastructure.UserInfo
{
    public class TokenInfoRetriever : IUserInfo
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TokenInfoRetriever(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal User => _httpContextAccessor.HttpContext.User;

        public bool IsLoggedIn => User.Identity.IsAuthenticated;

        public Guid UserId => Guid.Parse(User.FindFirstValue(JwtClaimNames.Subject));

        public IEnumerable<Role> Roles => User.FindAll(JwtClaimNames.Role).Select(c => Role.From(c.Value));

        public bool HasRole(Role role) => Roles.Contains(role);
    }
}
