using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.Sagas;
using EasyDesk.SampleApp.Application.Commands;

namespace EasyDesk.SampleApp.Application.Queries;

public record GetBulkCreatePetsStatus : IQueryRequest<CreatePetsStatusDto>;

public record CreatePetsStatusDto(bool InProgress);

public class GetBulkCreatePetsStatusHandler : IHandler<GetBulkCreatePetsStatus, CreatePetsStatusDto>
{
    private readonly ISagaManager _sagaManager;

    public GetBulkCreatePetsStatusHandler(ISagaManager sagaManager)
    {
        _sagaManager = sagaManager;
    }

    public async Task<Result<CreatePetsStatusDto>> Handle(GetBulkCreatePetsStatus request)
    {
        var inProgress = await BulkCreatePets.IsInProgress(_sagaManager);
        return new CreatePetsStatusDto(inProgress);
    }
}
