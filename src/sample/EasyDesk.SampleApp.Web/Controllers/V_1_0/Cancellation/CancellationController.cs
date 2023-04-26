using EasyDesk.CleanArchitecture.Web.Controllers;
using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.SampleApp.Application.Commands;
using Microsoft.AspNetCore.Mvc;
using NodaTime;

namespace EasyDesk.SampleApp.Web.Controllers.V_1_0.Cancellation;

public static class CancellationRoutes
{
    public const string CancellableRequest = "cancellable";
}

public class CancellationController : CleanArchitectureController
{
    [HttpPost(CancellationRoutes.CancellableRequest)]
    public async Task<ActionResult<ResponseDto<Nothing, Nothing>>> CancellableRequest(
        [FromQuery] Duration? waitTime = null)
    {
        return await Dispatch(new CancellableRequest(waitTime ?? Duration.Epsilon))
            .ReturnOk();
    }
}
