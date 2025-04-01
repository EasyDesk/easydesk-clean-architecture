using EasyDesk.CleanArchitecture.Application.Data;

namespace EasyDesk.CleanArchitecture.Application.DomainServices;

internal class NullSaveChangesHandler : ISaveChangesHandler
{
    public Task SaveChanges() => Task.CompletedTask;
}
