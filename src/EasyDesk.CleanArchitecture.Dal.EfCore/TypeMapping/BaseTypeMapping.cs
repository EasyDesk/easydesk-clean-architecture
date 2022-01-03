using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.TypeMapping;

public abstract class BaseTypeMapping<TModel, TProvider> : RelationalTypeMapping
{
    protected BaseTypeMapping(ValueConverter<TModel, TProvider> converter, string storeType)
        : this(new RelationalTypeMappingParameters(
            new CoreTypeMappingParameters(typeof(TModel), converter), storeType))
    {
    }

    protected BaseTypeMapping(RelationalTypeMappingParameters parameters) : base(parameters)
    {
    }
}
