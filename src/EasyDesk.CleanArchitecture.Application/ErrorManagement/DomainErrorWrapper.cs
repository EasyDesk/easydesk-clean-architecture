using EasyDesk.CleanArchitecture.Domain.Metamodel;

namespace EasyDesk.CleanArchitecture.Application.ErrorManagement
{
    public record DomainErrorWrapper(DomainError DomainError) : Error(
        $"Domain Error: {DomainError.GetType().Name}",
        DomainError.GetType().Name);
}
