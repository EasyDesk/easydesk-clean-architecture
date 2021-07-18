using System.Linq;

namespace EasyDesk.CleanArchitecture.Application.Utils
{
    public delegate IQueryable<T> QueryWrapper<T>(IQueryable<T> query);
}
