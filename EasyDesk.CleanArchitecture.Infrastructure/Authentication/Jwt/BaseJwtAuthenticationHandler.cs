using EasyDesk.CleanArchitecture.Infrastructure.Jwt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;

namespace EasyDesk.CleanArchitecture.Infrastructure.Authentication.Jwt
{
    public abstract class BaseJwtAuthenticationHandler<T> : BaseAuthenticationHandler<T>
        where T : AuthenticationSchemeOptions, new()
    {
        private readonly JwtService _jwtService;
        private readonly IConfiguration _config;

        protected BaseJwtAuthenticationHandler(
            IOptionsMonitor<T> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            string scheme,
            JwtService jwtService,
            IConfiguration config)
            : base(options, logger, encoder, clock, scheme)
        {
            _jwtService = jwtService;
            _config = config;
        }

        protected JwtScope GetJwtScope(string name)
        {
            return JwtUtils.CreateScopeFromNameAndSettings(_jwtService, _config, name);
        }
    }
}
