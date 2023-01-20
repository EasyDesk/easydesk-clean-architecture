using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Infrastructure.ContextProvider;
using Microsoft.AspNetCore.Http;
using Shouldly;

namespace EasyDesk.CleanArchitecture.UnitTests.Infrastructure.Authentication;

public class ContextProviderTests
{

    [Fact]
    public void ShouldDetectAuthentication_FromHttpContextAccessor()
    {
        var id = "some-user-id";
        var accessor = new HttpContextAccessor();
        accessor.SetupAuthenticatedHttpContext(id);
        var sut = new BasicContextProvider(accessor);
        sut.UserInfo.IsPresent.ShouldBeTrue();
        sut.Context.ShouldBe(new AuthenticatedRequestContext(sut.UserInfo.Value));
        sut.RequireUserInfo().UserId.ShouldBe(id);
    }
}
