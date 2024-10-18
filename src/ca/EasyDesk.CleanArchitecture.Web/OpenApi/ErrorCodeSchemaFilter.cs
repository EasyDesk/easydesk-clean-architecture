using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.CleanArchitecture.Infrastructure.ContextProvider.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.CleanArchitecture.Web.OpenApi.DependencyInjection;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Reflection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EasyDesk.CleanArchitecture.Web.OpenApi;

internal class ErrorCodeSchemaFilter : ISchemaFilter
{
    private readonly ISerializerDataContractResolver _serializerDataContractResolver;
    private readonly IEnumerable<string> _errorCodes;

    public ErrorCodeSchemaFilter(ISerializerDataContractResolver serializerDataContractResolver, AppDescription app)
    {
        _serializerDataContractResolver = serializerDataContractResolver;
        _errorCodes = new AssemblyScanner()
            .FromAssemblies(app.Assemblies)
            .FromAssembliesContaining(
                typeof(ApplicationError),
                typeof(ContextProviderModule),
                typeof(OpenApiModule))
            .NonAbstract()
            .SubtypesOrImplementationsOf<ApplicationError>()
            .FindTypes()
            .Select(ErrorDto.GetErrorCodeFromApplicationErrorType)
            .ToFixedSortedSet();
    }

    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var type = context.Type;
        if (!type.IsAssignableTo(typeof(ErrorDto)))
        {
            return;
        }
        var codeDataProperty = _serializerDataContractResolver
            .GetDataContractForType(typeof(ErrorDto))
            .ObjectProperties
            .First(p => p.MemberInfo == typeof(ErrorDto).GetProperty(nameof(ErrorDto.Code)));
        schema.Properties[codeDataProperty.Name] = new OpenApiSchema
        {
            Type = "string",
            Enum = _errorCodes.Select(code => new OpenApiString(code) as IOpenApiAny).ToList(),
            ReadOnly = true,
            Example = new OpenApiString("ErrorCode"),
        };
    }
}
