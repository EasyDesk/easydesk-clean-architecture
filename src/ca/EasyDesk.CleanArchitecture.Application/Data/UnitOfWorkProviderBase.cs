namespace EasyDesk.CleanArchitecture.Application.Data;

public abstract class UnitOfWorkProviderBase<T> : IUnitOfWorkProvider
    where T : IUnitOfWork
{
    protected Option<T> UnitOfWork { get; private set; }

    public Option<IUnitOfWork> CurrentUnitOfWork => UnitOfWork.Map(x => x as IUnitOfWork);

    public async Task<IUnitOfWork> BeginUnitOfWork()
    {
        if (CurrentUnitOfWork.IsPresent)
        {
            throw new InvalidOperationException("A unit of work was already started");
        }
        var unitOfWork = await CreateUnitOfWork();
        UnitOfWork = Some(unitOfWork);
        return unitOfWork;
    }

    protected abstract Task<T> CreateUnitOfWork();
}
