using EasyDesk.CleanArchitecture.Domain.Roles;
using EasyDesk.CleanArchitecture.Infrastructure.Jwt;
using EasyDesk.Tools.Collections;
using EasyDesk.Tools.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace EasyDesk.CleanArchitecture.Infrastructure.Authentication.ApiKey
{
    public class ApiKeyClaimsBuilder
    {
        private readonly List<Claim> _claims = new();

        public IEnumerable<Claim> Claims => _claims;

        private void AddClaim(string claimName, string value) => _claims.Add(new Claim(claimName, value));

        public void AddRole(Role role) => AddClaim(JwtClaimNames.Role, role.ToString());

        public void AddApiKeyName(string subject) => AddClaim(ApiKeyAuthenticationOptions.ApiKeyClaimName, subject);
    }

    public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
    {
        public const string ApiKeyClaimName = "ApiKey";

        private readonly IDictionary<string, IEnumerable<Claim>> _apiKeys;

        public ApiKeyAuthenticationOptions()
        {
            _apiKeys = new Dictionary<string, IEnumerable<Claim>>();
        }

        public void AddApiKey(string apiKey, Action<ApiKeyClaimsBuilder> claimsModifier = null)
        {
            var claimsBuilder = new ApiKeyClaimsBuilder();
            claimsModifier?.Invoke(claimsBuilder);
            _apiKeys.Add(apiKey, claimsBuilder.Claims);
        }

        public void AddApiKeyFromConfig(string apiKeyName, IConfiguration config, Action<ApiKeyClaimsBuilder> claimsModifier = null)
        {
            AddApiKey(config.GetValue<string>($"ApiKeys:{apiKeyName}"), claims =>
            {
                claimsModifier?.Invoke(claims);
                claims.AddApiKeyName(apiKeyName);
            });
        }

        public Option<IEnumerable<Claim>> GetClaims(string apiKey) => _apiKeys.GetOption(apiKey);
    }

    public class ApiKeyAuthenticationHandler : BaseAuthenticationHandler<ApiKeyAuthenticationOptions>
    {
        public ApiKeyAuthenticationHandler(
            IOptionsMonitor<ApiKeyAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock) : base(options, logger, encoder, clock, ApiKeyDefaults.Scheme)
        {
        }

        protected override Option<ClaimsPrincipal> GetClaimsPrincipalFromToken(string token)
        {
            return Options.GetClaims(token)
                .Map(claims => new ClaimsIdentity(claims, Scheme.Name))
                .Map(identity => new ClaimsPrincipal(identity));
        }
    }
}
