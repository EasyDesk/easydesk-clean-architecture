using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.Sagas;
using EasyDesk.SampleApp.Application.Commands;

namespace EasyDesk.SampleApp.Application.Queries;

public record GetBulkCreatePetsStatus : IQueryRequest<BulkCreatePetsStatus>;

public record BulkCreatePetsStatus(bool InProgress);

public class GetBulkCreatePetsStatusHandler : IHandler<GetBulkCreatePetsStatus, BulkCreatePetsStatus>
{
    private readonly ISagaManager _sagaManager;

    public GetBulkCreatePetsStatusHandler(ISagaManager sagaManager)
    {
        _sagaManager = sagaManager;
    }

    public async Task<Result<BulkCreatePetsStatus>> Handle(GetBulkCreatePetsStatus request)
    {
        var inProgress = await BulkCreatePets.IsInProgress(_sagaManager);
        return new BulkCreatePetsStatus(inProgress);
    }
}
