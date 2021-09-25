using EasyDesk.Tools.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EasyDesk.CleanArchitecture.Infrastructure.Jwt
{
    public class JwtScope
    {
        private readonly JwtService _jwtService;
        private readonly JwtSettings _jwtSettings;

        public JwtScope(JwtService jwtService, JwtSettings jwtSettings)
        {
            _jwtService = jwtService;
            _jwtSettings = jwtSettings;
        }

        public Option<ClaimsIdentity> Validate(string jwt, out JwtSecurityToken token, bool validateLifetime = true)
        {
            return _jwtService.Validate(jwt, _jwtSettings, out token, validateLifetime);
        }

        public string CreateToken(ClaimsIdentity identity, out JwtSecurityToken token)
        {
            return _jwtService.CreateToken(identity, _jwtSettings, out token);
        }
    }
}
