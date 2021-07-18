using EasyDesk.CleanArchitecture.Infrastructure.Jwt;
using EasyDesk.Tools;
using EasyDesk.Tools.Options;
using EasyDesk.Tools.PrimitiveTypes.DateAndTime;
using EasyDesk.Testing.Utils;
using Shouldly;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;
using static EasyDesk.Tools.Collections.EnumerableUtils;

namespace EasyDesk.CleanArchitecture.UnitTests.Infrastructure.Jwt
{
    public class JwtServiceTests
    {
        private readonly JwtService _sut;
        private readonly SettableTimestampProvider _timestampProvider;

        private readonly IEqualityComparer<Claim> _claimsComparer = EqualityComparers
            .FromProperties<Claim>(c => c.Type, c => c.Value);

        private readonly JwtSettings _settings = new(Duration.FromMinutes(5), KeyUtils.KeyFromString("abcdefghijklmnopqrstuvwxyz"));
        private readonly IEnumerable<Claim> _claims = Items(
                new Claim("claimA", "valueA"),
                new Claim("claimB", "valueB"));

        public JwtServiceTests()
        {
            _timestampProvider = new SettableTimestampProvider(Timestamp.Now);

            _sut = new(_timestampProvider);
        }

        private string CreateToken(out JwtSecurityToken token) => _sut.CreateToken(new(_claims), _settings, out token);

        private Option<ClaimsIdentity> Validate(string jwt, out JwtSecurityToken token) => _sut.Validate(jwt, _settings, out token);

        [Fact]
        public void CreateToken_ShouldReturnATokenWithTheGivenClaims()
        {
            CreateToken(out var token);
            _claims.ShouldBeSubsetOf(token.Claims, _claimsComparer);
        }

        [Fact]
        public void CreateToken_ShouldReturnATokenWithTheProperExpiration()
        {
            CreateToken(out var token);

            token.ValidTo.ShouldBe(_timestampProvider.Now.AsDateTime + _settings.Lifetime.AsTimeSpan, tolerance: TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Validate_ShouldFail_IfTheTokenIsBadlyFormatted()
        {
            Validate("BADLY FORMATTED TOKEN", out _).ShouldBeEmpty();
        }

        [Fact]
        public void Validate_ShouldFail_IfTheTokenWasSignedWithADifferentKey()
        {
            var jwt = _sut.CreateToken(new(_claims), _settings with { Key = KeyUtils.KeyFromString("qwertyuiopasdfghjklzxcvbnm") }, out _);
            
            Validate(jwt, out _).ShouldBeEmpty();
        }

        [Fact]
        public void Validate_ShouldFail_IfTheTokenHasExpired()
        {
            var jwt = CreateToken(out _);

            _timestampProvider
                .Advance(_settings.Lifetime)
                .Advance(JwtService.ClockSkew)
                .AdvanceBySeconds(1);

            Validate(jwt, out _).ShouldBeEmpty();
        }

        [Fact]
        public void Validate_ShouldSucceed_IfTheTokenIsValid()
        {
            var jwt = CreateToken(out _);

            Validate(jwt, out _).ShouldNotBeEmpty();
        }

        [Fact]
        public void Validate_ShouldExtractTheSameClaimsAsTheCreatedToken()
        {
            var jwt = CreateToken(out var token);

            Validate(jwt, out var validated);

            validated.Claims.ShouldBe(token.Claims, _claimsComparer, ignoreOrder: true);
        }

        [Fact]
        public void Validate_ShouldReturnAClaimsIdentityWithTheClaimsUsedOnTokenGeneration()
        {
            var jwt = CreateToken(out _);

            var identity = Validate(jwt, out _);

            _claims.ShouldBeSubsetOf(identity.Value.Claims, _claimsComparer);
        }
    }
}
