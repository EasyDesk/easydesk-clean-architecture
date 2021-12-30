using EasyDesk.CleanArchitecture.Infrastructure.Jwt;
using EasyDesk.Tools.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace EasyDesk.CleanArchitecture.Web.Authentication.Jwt
{
    public class JwtBearerOptions : AuthenticationSchemeOptions
    {
        public JwtSettings JwtSettings { get; private set; }

        public void UseJwtSettingsFromConfiguration(IConfiguration configuration, string scopeName = "Default") =>
            JwtSettings = configuration.ReadJwtSettings(scopeName);

        public void UseJwtSettings(JwtSettings settings) => JwtSettings = settings;
    }

    public class JwtBearerHandler : BaseAuthenticationHandler<JwtBearerOptions>
    {
        private readonly JwtService _jwtService;

        public JwtBearerHandler(
            IOptionsMonitor<JwtBearerOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            JwtService jwtService)
            : base(options, logger, encoder, clock, JwtBearerDefaults.AuthenticationScheme)
        {
            _jwtService = jwtService;
        }

        protected override Option<ClaimsPrincipal> GetClaimsPrincipalFromToken(string token)
        {
            var scope = new JwtScope(_jwtService, Options.JwtSettings);
            return scope.Validate(token, out var _).Map(x => new ClaimsPrincipal(x));
        }
    }
}
