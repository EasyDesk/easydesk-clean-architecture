using EasyDesk.CleanArchitecture.Application.Json;
using EasyDesk.CleanArchitecture.Application.Sagas;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Options;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Sagas;

internal class EfCoreSagaManager : ISagaManager
{
    private readonly SagasContext _context;
    private readonly JsonSerializerOptions _serializerOptions;

    public EfCoreSagaManager(SagasContext context, JsonOptionsConfigurator jsonConfigurator)
    {
        _context = context;
        _serializerOptions = jsonConfigurator.CreateOptions();
    }

    private IQueryable<SagaModel> QuerySaga<TId, TState>(TId id) =>
        _context.Sagas.Where(s => s.Id == GetSagaIdAsString(id) && s.Type == FormatSagaType<TState>());

    public async Task<Option<(ISagaReference<TState> Reference, TState State)>> Find<TId, TState>(TId id)
    {
        return await QuerySaga<TId, TState>(id)
            .FirstOptionAsync()
            .ThenMap(m => (CreateReferenceFromSagaModel<TState>(m), JsonSerializer.Deserialize<TState>(m.State, _serializerOptions)));
    }

    public async Task<bool> IsInProgress<TId, TState>(TId id) =>
        await QuerySaga<TId, TState>(id).AnyAsync();

    public ISagaReference<TState> CreateNew<TId, TState>(TId id)
    {
        var sagaModel = new SagaModel
        {
            Id = GetSagaIdAsString(id),
            Type = FormatSagaType<TState>(),
            Version = 1,
        };
        _context.Sagas.Add(sagaModel);
        return CreateReferenceFromSagaModel<TState>(sagaModel);
    }

    private string GetSagaIdAsString<TId>(TId id) =>
        id?.ToString() ?? throw new InvalidOperationException($"{typeof(TId)}.ToString returned null.");

    private string FormatSagaType<T>() => FormatSagaType(typeof(T));

    private string FormatSagaType(Type type) => $"{type.Name}{FormatSagaTypeGenerics(type)}";

    private string FormatSagaTypeGenerics(Type type) =>
        type.IsGenericType ? type.GetGenericArguments().Select(FormatSagaType).ConcatStrings(",", "<", ">") : string.Empty;

    private ISagaReference<TState> CreateReferenceFromSagaModel<TState>(SagaModel sagaModel) =>
        new EfCoreSagaReference<TState>(sagaModel, _context, _serializerOptions);

    public async Task SaveAll() => await _context.SaveChangesAsync();
}

internal class EfCoreSagaReference<TState> : ISagaReference<TState>
{
    private readonly SagaModel _sagaModel;
    private readonly SagasContext _context;
    private readonly JsonSerializerOptions _serializerOptions;

    public EfCoreSagaReference(SagaModel sagaModel, SagasContext context, JsonSerializerOptions serializerOptions)
    {
        _sagaModel = sagaModel;
        _context = context;
        _serializerOptions = serializerOptions;
    }

    public void UpdateState(TState state)
    {
        _sagaModel.State = JsonSerializer.Serialize(state, _serializerOptions);
        _sagaModel.Version++;
    }

    public void Delete()
    {
        _context.Sagas.Remove(_sagaModel);
    }
}
