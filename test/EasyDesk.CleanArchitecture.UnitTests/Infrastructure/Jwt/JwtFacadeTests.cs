﻿using System.Security.Claims;
using EasyDesk.CleanArchitecture.Infrastructure.Jwt;
using Microsoft.IdentityModel.Tokens;
using NodaTime;
using NodaTime.Testing;
using Shouldly;
using Xunit;

namespace EasyDesk.CleanArchitecture.UnitTests.Infrastructure.Jwt;

public class JwtFacadeTests
{
    private const string Audience = "test-audience";
    private const string Issuer = "test-issuer";
    private const string Algorithm = SecurityAlgorithms.HmacSha256Signature;
    private const string KeyId = "test-kid";

    private readonly FakeClock _clock = new(SystemClock.Instance.GetCurrentInstant());
    private readonly SecurityKey _key = KeyUtils.KeyFromString("abcdefghijklmnopqrstuvwxyz", KeyId);
    private readonly Duration _lifetime = Duration.FromMinutes(5);
    private readonly JwtTokenConfiguration _configureJwtToken;
    private readonly JwtValidationConfiguration _configureDefaultValidation;
    private readonly JwtFacade _sut;

    private readonly IEnumerable<Claim> _claims = new Claim[]
    {
        new("claimA", "valueA"),
        new("claimB", "valueB")
    };

    private readonly IEqualityComparer<Claim> _claimsComparer = EqualityComparers
        .FromProperties<Claim>(c => c.Type, c => c.Value);

    public JwtFacadeTests()
    {
        _configureJwtToken = builder => builder
            .WithSigningCredentials(_key, Algorithm)
            .WithLifetime(_lifetime)
            .WithIssuer(Issuer)
            .WithAudience(Audience);

        _configureDefaultValidation = builder => builder
            .WithSignatureValidation(_key);

        _sut = new(_clock);
    }

    [Fact]
    public void NoneAlgorithmAttackShouldNotBePossible()
    {
        var jwt = _sut.Create(_claims, _configureJwtToken);
        var jwtWithoutHeader = jwt[jwt.IndexOf('.')..];
        var replacedHeader = Base64UrlEncoder.Encode($@"{{""alg"":""none"",""typ"":""jwt"",""kid"":""{KeyId}""}}");
        var forgedJwt = replacedHeader + jwtWithoutHeader;
        _sut.Validate(forgedJwt, _configureDefaultValidation).ShouldBeEmpty();
    }

    [Fact]
    public void ShouldCreateATokenWithTheProperExpiration()
    {
        _sut.Create(_claims, out var token, _configureJwtToken);

        token.ValidTo.ShouldBe(
            (_clock.GetCurrentInstant() + _lifetime).ToDateTimeUtc(),
            tolerance: TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ShouldFailValidatingBadlyFormattedTokens()
    {
        _sut.Validate("BADLY FORMATTED TOKEN", _configureDefaultValidation).ShouldBeEmpty();
    }

    [Fact]
    public void ShouldFailValidatingTokenSignedWithADifferentKey()
    {
        var jwt = _sut.Create(_claims, _configureJwtToken);

        _sut.Validate(jwt, b => b.WithSignatureValidation(KeyUtils.KeyFromString("qwertyuiopasdfghjklzxcvbnm", KeyId))).ShouldBeEmpty();
    }

    [Fact]
    public void ShouldFailValidatingExpiredTokens()
    {
        var jwt = _sut.Create(_claims, _configureJwtToken);

        AdvanceToPostExpiration();

        _sut.Validate(jwt, _configureDefaultValidation).ShouldBeEmpty();
    }

    [Fact]
    public void ShouldSucceedValidatingCorrectTokens()
    {
        var jwt = _sut.Create(_claims, _configureJwtToken);

        _sut.Validate(jwt, _configureDefaultValidation).ShouldNotBeEmpty();
    }

    [Fact]
    public void ShouldKeepTheOriginalClaimsAfterValidation()
    {
        var jwt = _sut.Create(_claims, _configureJwtToken);
        var identity = _sut.Validate(jwt, _configureDefaultValidation).Value;
        _claims.ShouldBeSubsetOf(identity.Claims, _claimsComparer);
    }

    [Fact]
    public void ShouldFailIfTheIssuerIsNotValid()
    {
        var jwt = _sut.Create(_claims, _configureJwtToken);
        _sut.Validate(jwt, b =>
        {
            _configureDefaultValidation(b);
            b.WithIssuerValidation("another-issuer");
        }).ShouldBeEmpty();
    }

    [Fact]
    public void ShouldFailIfTheAudienceIsNotValid()
    {
        var jwt = _sut.Create(_claims, _configureJwtToken);
        _sut.Validate(jwt, b =>
        {
            _configureDefaultValidation(b);
            b.WithAudienceValidation("another-audience");
        }).ShouldBeEmpty();
    }

    [Fact]
    public void ShouldIgnoreLifetimeValidationIfExplicitlyRequested()
    {
        var jwt = _sut.Create(_claims, _configureJwtToken);

        AdvanceToPostExpiration();

        _sut.Validate(jwt, b =>
        {
            _configureDefaultValidation(b);
            b.WithoutLifetimeValidation();
        }).ShouldNotBeEmpty();
    }

    [Fact]
    public void ShouldSucceedIfAllAdditionalRequirementsAreMet()
    {
        var jwt = _sut.Create(_claims, _configureJwtToken);

        var result = _sut.Validate(jwt, b =>
        {
            _configureDefaultValidation(b);
            b.WithAudienceValidation(Audience).WithIssuerValidation(Issuer);
        });

        result.ShouldNotBeEmpty();
    }

    private void AdvanceToPostExpiration()
    {
        _clock.Advance(_lifetime);
        _clock.Advance(Duration.FromSeconds(1));
    }
}