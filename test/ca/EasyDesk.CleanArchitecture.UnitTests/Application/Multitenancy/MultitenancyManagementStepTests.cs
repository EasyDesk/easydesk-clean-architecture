using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.Testing.Errors;
using EasyDesk.Testing.MatrixExpansion;
using NSubstitute;
using Shouldly;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.Multitenancy;

public class MultitenancyManagementStepTests
{
    private record DefaultPolicyRequest;

    private record OverriddenPolicyRequest(MultitenantPolicy MultitenantPolicy) : IOverrideMultitenantPolicy
    {
        public MultitenantPolicy GetMultitenantPolicy() => MultitenantPolicy;
    }

    private const string InvalidTenantId = "###invalid###";
    private static readonly TenantId _tenantId = TenantId.New("test-tenant");

    private readonly IContextTenantReader _tenantReader;
    private readonly IMultitenancyManager _multitenancyManager;
    private readonly IContextTenantInitializer _tenantInitializer;
    private readonly NextPipelineStep<Nothing> _next;

    private readonly MultitenantPolicy _defaultPolicy;

    public MultitenancyManagementStepTests()
    {
        _tenantReader = Substitute.For<IContextTenantReader>();
        _tenantReader.GetTenantId().Returns(None);

        _tenantInitializer = Substitute.For<IContextTenantInitializer>();

        _multitenancyManager = Substitute.For<IMultitenancyManager>();

        _next = Substitute.For<NextPipelineStep<Nothing>>();
        _next().Returns(Ok);

        _defaultPolicy = Substitute.For<MultitenantPolicy>();
        _defaultPolicy(Arg.Any<TenantInfo>(), _multitenancyManager)
            .Returns(call => call.Arg<TenantInfo>());
    }

    private void UseRawContextTenantId(Option<string> rawTenantId) => _tenantReader.GetTenantId().Returns(rawTenantId);

    private async Task<Result<Nothing>> Run<T>(T request)
    {
        var step = new MultitenancyManagementStep<T, Nothing>(
            _tenantReader,
            _tenantInitializer,
            _multitenancyManager,
            _defaultPolicy);

        return await step.Run(request, _next);
    }

    private async Task ShouldSucceed<T>(T request, TenantInfo expectedTenantInfo)
    {
        var result = await Run(request);
        result.ShouldBe(Ok);
        _tenantInitializer.Received(1).Initialize(expectedTenantInfo);
        await _next.Received(1)();
    }

    private async Task ShouldFail<T>(T request, Error expectedError)
    {
        var result = await Run(request);
        result.ShouldBe(expectedError);
        _tenantInitializer.DidNotReceiveWithAnyArgs().Initialize(default!);
        await _next.DidNotReceiveWithAnyArgs()();
    }

    [Fact]
    public async Task ShouldFailWhenContextTenantIsInvalid()
    {
        UseRawContextTenantId(Some(InvalidTenantId));
        await ShouldFail(new DefaultPolicyRequest(), new InvalidTenantIdError(InvalidTenantId));
    }

    [Theory]
    [MemberData(nameof(Tenants))]
    public async Task ShouldFailWhenPolicyDoesNotAllowTheGivenTenant(TenantInfo tenantInfo)
    {
        var error = TestError.Create();
        UseRawContextTenantId(GetRawTenantId(tenantInfo));
        _defaultPolicy(tenantInfo, _multitenancyManager).Returns(error);

        await ShouldFail(new DefaultPolicyRequest(), error);
    }

    [Theory]
    [MemberData(nameof(Tenants))]
    public async Task ShouldInitializeTheContextTenantWithTheTenantInfoFromThePolicy(TenantInfo tenantInfo)
    {
        _defaultPolicy(Arg.Any<TenantInfo>(), _multitenancyManager).Returns(tenantInfo);

        await ShouldSucceed(new DefaultPolicyRequest(), tenantInfo);
    }

    [Theory]
    [MemberData(nameof(TenantsAndPolicyResults))]
    public async Task ShouldUseTheOverriddenPolicyWhenSpecified(TenantInfo tenantInfo, Result<TenantInfo> policyResult)
    {
        UseRawContextTenantId(GetRawTenantId(tenantInfo));
        var policy = Substitute.For<MultitenantPolicy>();
        policy(tenantInfo, _multitenancyManager).Returns(policyResult);

        var result = await Run(new OverriddenPolicyRequest(policy));

        await policy.Received(1)(tenantInfo, _multitenancyManager);
        result.ShouldBe(policyResult);
    }

    private Option<string> GetRawTenantId(TenantInfo tenantInfo) => tenantInfo.Id.Map(i => i.Value);

    public static IEnumerable<object[]> Tenants()
    {
        yield return new object[] { TenantInfo.Tenant(_tenantId) };
        yield return new object[] { TenantInfo.Public };
    }

    public static IEnumerable<object[]> TenantsAndPolicyResults()
    {
        return Matrix.Builder()
            .Axis(
                TenantInfo.Tenant(_tenantId),
                TenantInfo.Public)
            .Axis(
                Success(TenantInfo.Tenant(_tenantId)),
                Success(TenantInfo.Public),
                Failure<TenantInfo>(TestError.Create()))
            .Build();
    }
}
