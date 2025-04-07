using EasyDesk.Commons.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Data.Sql;

public record TableDef(string Schema, string Name, IFixedList<string> Columns);
