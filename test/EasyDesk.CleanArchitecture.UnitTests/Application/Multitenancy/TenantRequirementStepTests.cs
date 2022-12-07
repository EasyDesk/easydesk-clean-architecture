using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using NSubstitute;
using Shouldly;
using Xunit;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.Multitenancy;

public class TenantRequirementStepTests
{
    private record DefaultPolicyRequest;

    [UseMultitenantPolicy(MultitenantPolicy.AllowAll)]
    private record AllowAllRequest;

    [UseMultitenantPolicy(MultitenantPolicy.RequireTenant)]
    private record RequireTenantRequest;

    [UseMultitenantPolicy(MultitenantPolicy.RequireNoTenant)]
    private record RequireNoTenantRequest;

    private const string TenantId = "test";

    private readonly ITenantProvider _tenantProvider;
    private readonly NextPipelineStep<Nothing> _next;

    private readonly MultitenancyOptions _options = new();

    public TenantRequirementStepTests()
    {
        _tenantProvider = Substitute.For<ITenantProvider>();
        _tenantProvider.TenantId.Returns(None);

        _next = Substitute.For<NextPipelineStep<Nothing>>();
        _next().Returns(Ok);
    }

    private void EnterTenant() => _tenantProvider.TenantId.Returns(Some(TenantId));

    private async Task<Result<Nothing>> Run<T>() where T : new()
    {
        return await new TenantRequirementStep<T, Nothing>(_tenantProvider, _options)
            .Run(new T(), _next);
    }

    private async Task ShouldBeAllowed<T>() where T : new()
    {
        var result = await Run<T>();

        await _next.Received(1)();
        result.ShouldBe(Ok);
    }

    private async Task ShouldNotBeAllowed<T>(Error expectedError) where T : new()
    {
        var result = await Run<T>();

        result.ShouldBe(expectedError);
        await _next.DidNotReceiveWithAnyArgs()();
    }

    [Fact]
    public async Task AllowAll_ShouldAllowRequestsWithinATenant()
    {
        EnterTenant();
        await ShouldBeAllowed<AllowAllRequest>();
    }

    [Fact]
    public async Task AllowAll_ShouldAllowRequestsWithoutATenant()
    {
        await ShouldBeAllowed<AllowAllRequest>();
    }

    [Fact]
    public async Task RequireTenant_ShouldAllowRequestsWithinATenant()
    {
        EnterTenant();
        await ShouldBeAllowed<RequireTenantRequest>();
    }

    [Fact]
    public async Task RequireTenant_ShouldNotAllowRequestsWithoutATenant()
    {
        await ShouldNotBeAllowed<RequireTenantRequest>(new MissingTenantError());
    }

    [Fact]
    public async Task RequireNoTenant_ShouldNotAllowRequestsWithinATenant()
    {
        EnterTenant();
        await ShouldNotBeAllowed<RequireNoTenantRequest>(new MultitenancyNotSupportedError());
    }

    [Fact]
    public async Task RequireNoTenant_ShouldAllowRequestsWithoutATenant()
    {
        await ShouldBeAllowed<RequireNoTenantRequest>();
    }

    [Fact]
    public async Task ShouldUseDefaultPolicyIfRequestHasNoAttributeSpecified()
    {
        _options.DefaultPolicy = MultitenantPolicy.RequireTenant;

        await ShouldNotBeAllowed<DefaultPolicyRequest>(new MissingTenantError());
    }
}
