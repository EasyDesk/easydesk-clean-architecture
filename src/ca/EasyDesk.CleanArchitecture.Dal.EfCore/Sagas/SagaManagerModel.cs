using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Sagas;

public static class SagaManagerModel
{
    public const string SchemaName = "sagas";

    public const int SagaTypeMaxLength = EfCoreUtils.SymbolMaxLength;
}
