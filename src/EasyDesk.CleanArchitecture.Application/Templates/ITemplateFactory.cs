using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Templates;

public interface ITemplateFactory
{
    Task<string> Render<T>(string key, T model);

    Task<string> RenderRaw<T>(string key, string content, T model);
}
