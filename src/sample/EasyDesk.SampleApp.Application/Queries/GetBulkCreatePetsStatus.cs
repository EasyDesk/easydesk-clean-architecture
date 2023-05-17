using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.Sagas;
using EasyDesk.CleanArchitecture.Application.Sagas.BulkOperations;
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
        var inProgress = await _sagaManager
            .IsInProgress<string, BulkOperationState<CreatePetsResult, IEnumerable<CreatePet>>>(typeof(BulkCreatePets).Name);
        return new BulkCreatePetsStatus(inProgress);
    }
}
