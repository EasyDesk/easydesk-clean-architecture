using EasyDesk.CleanArchitecture.Infrastructure.Jwt;
using EasyDesk.CleanArchitecture.Web.Authentication.Jwt;
using Microsoft.IdentityModel.Tokens;
using NodaTime;
using NodaTime.Testing;
using System.Security.Claims;
using static EasyDesk.Commons.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.UnitTests.Infrastructure.Jwt;

[UsesVerify]
public class JwtValidationTests
{
    private readonly JwtFacade _jwtFacade;
    private readonly FakeClock _clock;

    public JwtValidationTests()
    {
        _clock = new(Instant.FromUnixTimeTicks(0));
        _jwtFacade = new(_clock);
    }

    [Fact]
    public async Task ShouldConvertValidationToGenerationConfigurationCorrectly()
    {
        var validation = new JwtValidationConfiguration(
            KeyUtils.KeyFromString("IUYTFCVBNJUHDNEJNUNWOCNJVJNWRNHIUYFQWEZXVCPIOUBN"),
            Set("Issuer"),
            Set("Audience"),
            Enumerable.Empty<SecurityKey>());

        var generation = validation.ToJwtGenerationConfiguration();

        var jwt = _jwtFacade.Create(new(new[] { new Claim("claim", "value") }), generation.ConfigureBuilder);

        await Verify(jwt);
    }
}
