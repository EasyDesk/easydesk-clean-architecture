namespace EasyDesk.CleanArchitecture.Application.Cqrs;

public interface ICommand<T> : ICqrsRequest<T>
{
}
