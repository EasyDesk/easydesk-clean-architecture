using Neuroglia.AsyncApi.Configuration;
using Neuroglia.AsyncApi.Models;
using Neuroglia.AsyncApi.Services.Generators;

namespace EasyDesk.CleanArchitecture.Web.Neuroglia;
public class AsyncApiDocumentGenerator : IAsyncApiDocumentGenerator
{
    public Task<IEnumerable<AsyncApiDocument>> GenerateAsync(params Type[] markupTypes)
    {
        return this.GenerateAsync(markupTypes, null);
    }

    public Task<IEnumerable<AsyncApiDocument>> GenerateAsync(IEnumerable<Type> markupTypes, AsyncApiDocumentGenerationOptions options)
    {
        if (markupTypes == null)
        {
            throw new ArgumentNullException(nameof(markupTypes));
        }
        IEnumerable<Type> types = markupTypes
            .Select(t => t.Assembly)
            .Distinct()
            .SelectMany(t => t.GetTypes())
            .Where(t => true);
        return null;
    }
}
