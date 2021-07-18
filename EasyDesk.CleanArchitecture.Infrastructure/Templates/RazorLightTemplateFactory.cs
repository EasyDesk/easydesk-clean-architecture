using EasyDesk.CleanArchitecture.Application.Templates;
using RazorLight;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Infrastructure.Templates
{
    public class RazorLightTemplateFactory : ITemplateFactory
    {
        private readonly IRazorLightEngine _razorEngine;

        public RazorLightTemplateFactory(IRazorLightEngine razorEngine)
        {
            _razorEngine = razorEngine;
        }

        public Task<string> Render<T>(string key, T model) => _razorEngine.CompileRenderAsync(key, model);

        public Task<string> RenderRaw<T>(string key, string content, T model) => _razorEngine.CompileRenderStringAsync(key, content, model);
    }
}
