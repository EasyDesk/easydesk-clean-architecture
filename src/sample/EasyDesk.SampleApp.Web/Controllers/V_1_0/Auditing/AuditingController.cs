using EasyDesk.CleanArchitecture.Application.Auditing;
using EasyDesk.CleanArchitecture.Web.Controllers;
using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.SampleApp.Application.V_1_0.Queries;
using Microsoft.AspNetCore.Mvc;
using NodaTime;

namespace EasyDesk.SampleApp.Web.Controllers.V_1_0.Auditing;

public static class AuditingRoutes
{
    public const string GetAudits = "audits";
}

public class AuditingController : CleanArchitectureController
{
    [HttpGet(AuditingRoutes.GetAudits)]
    public async Task<ActionResult<ResponseDto<IEnumerable<AuditRecordDto>, PaginationMetaDto>>> GetAudits(
        [FromQuery] PaginationDto pagination,
        [FromQuery] string? name = null,
        [FromQuery] string? userId = null,
        [FromQuery] bool? anonymous = null,
        [FromQuery] bool? success = null,
        [FromQuery] string? type = null,
        [FromQuery] Instant? from = null,
        [FromQuery] Instant? to = null)
    {
        var query = new AuditQuery
        {
            MatchName = name.AsOption(),
            MatchUserId = userId.AsOption(),
            IsAnonymous = anonymous.AsOption(),
            IsSuccess = success.AsOption(),
            MatchType = type.AsOption().Map(Enum.Parse<AuditRecordType>),
            FromInstant = from.AsOption(),
            ToInstant = to.AsOption(),
        };
        return await DispatchWithPagination(new GetAudits(query), pagination)
            .ReturnOk();
    }
}
