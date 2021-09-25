using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.Tools;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Events.ExternalEvents
{
    public interface IExternalEventHandler
    {
        Task<Response<Nothing>> Handle(IExternalEvent ev);
    }
}
