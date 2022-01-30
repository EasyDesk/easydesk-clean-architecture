using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Data;

public interface IUnitOfWork
{
    Task Save();
}
