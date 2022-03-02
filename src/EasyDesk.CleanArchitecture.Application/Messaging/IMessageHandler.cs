using EasyDesk.Tools;
using EasyDesk.Tools.Results;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Messaging;

public interface IMessageHandler<M> where M : IMessage
{
    Task<Result<Nothing>> Handle(M message);
}
