using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

namespace EasyDesk.CleanArchitecture.Infrastructure.Jwt
{
    public static class KeyUtils
    {
        public static SecurityKey KeyFromString(string rawKey)
        {
            var encodedKey = Encoding.ASCII.GetBytes(rawKey);
            return new SymmetricSecurityKey(encodedKey);
        }

        public static SecurityKey RandomKey() => KeyFromString(Guid.NewGuid().ToString());
    }
}
