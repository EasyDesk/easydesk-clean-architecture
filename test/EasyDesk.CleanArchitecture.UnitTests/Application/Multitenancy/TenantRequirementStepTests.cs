using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using NSubstitute;
using Shouldly;
using Xunit;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.Multitenancy;

public class TenantRequirementStepTests
{
    private record RequestWithTenantRequirement;

    [AllowNoTenant]
    private record RequestWithoutTenantRequirement;

    private const string TenantId = "test";

    private readonly ITenantProvider _tenantProvider;
    private readonly NextPipelineStep<Nothing> _next;

    public TenantRequirementStepTests()
    {
        _tenantProvider = Substitute.For<ITenantProvider>();
        _tenantProvider.TenantId.Returns(None);

        _next = Substitute.For<NextPipelineStep<Nothing>>();
    }

    private async Task<Result<Nothing>> Run<T>() where T : new()
    {
        return await new TenantRequirementStep<T, Nothing>(_tenantProvider)
            .Run(new T(), _next);
    }

    [Fact]
    public async Task ShouldAllowRequestsWithinATenant()
    {
        _tenantProvider.TenantId.Returns(Some(TenantId));

        await Run<RequestWithTenantRequirement>();

        await _next.Received(1)();
    }

    [Fact]
    public async Task ShouldNotAllowRequestsWithoutATenant()
    {
        await Run<RequestWithTenantRequirement>();

        await _next.DidNotReceiveWithAnyArgs()();
    }

    [Fact]
    public async Task ShouldReturnAnErrorWhenOutsideATenant()
    {
        var result = await Run<RequestWithTenantRequirement>();

        result.ShouldBe(new MissingTenantError());
    }

    [Fact]
    public async Task ShouldNotRequireATenantIfRequirementIsOptedOut()
    {
        await Run<RequestWithoutTenantRequirement>();

        await _next.Received(1)();
    }
}
