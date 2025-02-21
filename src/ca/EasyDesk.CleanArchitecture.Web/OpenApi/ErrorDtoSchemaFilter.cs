using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.CleanArchitecture.Infrastructure.Context.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.CleanArchitecture.Web.OpenApi.DependencyInjection;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Reflection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EasyDesk.CleanArchitecture.Web.OpenApi;

internal class ErrorDtoSchemaFilter : ISchemaFilter
{
    private readonly ISerializerDataContractResolver _serializerDataContractResolver;
    private readonly IEnumerable<string> _errorCodes;

    public ErrorDtoSchemaFilter(ISerializerDataContractResolver serializerDataContractResolver, AppDescription app)
    {
        _serializerDataContractResolver = serializerDataContractResolver;
        var errors = new AssemblyScanner()
            .FromAssemblies(app.Assemblies)
            .FromAssembliesContaining(
                typeof(ApplicationError),
                typeof(ContextModule),
                typeof(OpenApiModule))
            .NonAbstract()
            .SubtypesOrImplementationsOf<ApplicationError>()
            .FindTypes();
        _errorCodes = errors
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
        var errorDtoProperties = _serializerDataContractResolver
            .GetDataContractForType(typeof(ErrorDto))
            .ObjectProperties;
        var codeDataProperty = errorDtoProperties
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
