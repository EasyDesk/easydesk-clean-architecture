using EasyDesk.CleanArchitecture.Infrastructure.Multitenancy;
using EasyDesk.CleanArchitecture.Web.Authentication;
using EasyDesk.Tools.Collections;
using EasyDesk.Tools.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using static EasyDesk.Tools.Options.OptionImports;

namespace EasyDesk.SampleApp.Web.Authentication;

public class TestAuthOptions : TokenAuthenticationOptions
{
}

public class TestAuthHandler : TokenAuthenticationHandler<TestAuthOptions>
{
    private static readonly Dictionary<string, string> _claimTypeMap = new()
    {
        ["userId"] = ClaimTypes.NameIdentifier,
        ["tenantId"] = HttpContextExtensions.TenantIdClaimName
    };

    public TestAuthHandler(
        IOptionsMonitor<TestAuthOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock) : base(options, logger, encoder, clock)
    {
    }

    protected override Option<ClaimsPrincipal> GetClaimsPrincipalFromToken(string token)
    {
        var claims = token.Split(";", System.StringSplitOptions.RemoveEmptyEntries)
            .SelectMany(p => ConvertTokenPartToClaim(p));
        return Some(new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType: "Test")));
    }

    private Option<Claim> ConvertTokenPartToClaim(string part)
    {
        var match = Regex.Match(part, @"^(.+)=(.+)$");
        if (!match.Success)
        {
            return None;
        }
        var claimValue = match.Groups[2].Value;
        return _claimTypeMap.GetOption(match.Groups[1].Value).Map(claimType => new Claim(claimType, claimValue));
    }
}
