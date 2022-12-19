using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace EasyDesk.CleanArchitecture.Infrastructure.Jwt;

public static class KeyUtils
{
    public static SecurityKey KeyFromString(string rawKey, string keyId = null)
    {
        var encodedKey = Encoding.UTF8.GetBytes(rawKey);
        var securityKey = new SymmetricSecurityKey(encodedKey);
        if (keyId is not null)
        {
            securityKey.KeyId = keyId;
        }
        return securityKey;
    }

    public static SecurityKey RandomKey() => KeyFromString(Guid.NewGuid().ToString());
}
