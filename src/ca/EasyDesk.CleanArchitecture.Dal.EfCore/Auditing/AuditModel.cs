using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Auditing;

public static class AuditModel
{
    public const string SchemaName = "audit";

    public const int NameMaxLength = Constants.SymbolMaxLength;
}
