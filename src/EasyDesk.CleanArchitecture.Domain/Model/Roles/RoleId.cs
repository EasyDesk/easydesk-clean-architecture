using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.CleanArchitecture.Domain.Metamodel.Values;
using System.Text.RegularExpressions;

namespace EasyDesk.CleanArchitecture.Domain.Model.Roles;

public record RoleId : ValueWrapper<string, RoleId>
{
    public const string Pattern = @"^[A-Za-z0-9_]+$";

    private RoleId(string value) : base(value)
    {
        DomainConstraints.Check()
            .If(Regex.IsMatch(Value, Pattern), () => new InvalidRoleId())
            .OtherwiseThrowException();
    }

    public static RoleId From(string value) => new(value);
}

public record InvalidRoleId : DomainError;
