using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.Tools;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Data
{
    public interface IUnitOfWork
    {
        Task<Response<Nothing>> Save();
    }
}
