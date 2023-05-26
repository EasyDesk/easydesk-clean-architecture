using EasyDesk.CleanArchitecture.Testing.Integration.Http;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Paginated;
using EasyDesk.SampleApp.Application.V_1_0.Queries;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.Auditing;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Api;

public static class AuditingEndpoints
{
    public static HttpPaginatedRequestExecutor<AuditRecordDto> GetAudits(this HttpTestHelper http) =>
        http.GetPaginated<AuditRecordDto>(AuditingRoutes.GetAudits);
}
