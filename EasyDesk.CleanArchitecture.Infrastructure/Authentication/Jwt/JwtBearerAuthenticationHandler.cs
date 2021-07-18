using EasyDesk.CleanArchitecture;
using EasyDesk.CleanArchitecture.Infrastructure.Jwt;
using EasyDesk.Tools.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace EasyDesk.CleanArchitecture.Infrastructure.Authentication.Jwt
{
    public class JwtBearerAuthenticationOptions : AuthenticationSchemeOptions
    {
    }

    public class JwtBearerAuthenticationHandler : BaseJwtAuthenticationHandler<JwtBearerAuthenticationOptions>
    {
        public JwtBearerAuthenticationHandler(
            IOptionsMonitor<JwtBearerAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            JwtService jwtService,
            IConfiguration config)
            : base(options, logger, encoder, clock, JwtBearerDefaults.AuthenticationScheme, jwtService, config)
        {
        }

        protected override Option<ClaimsPrincipal> GetClaimsPrincipalFromToken(string token)
        {
            return GetJwtScope(JwtScopeNames.Global)
                .Validate(token, out var _)
                .Map(x => new ClaimsPrincipal(x));
        }
    }
}
