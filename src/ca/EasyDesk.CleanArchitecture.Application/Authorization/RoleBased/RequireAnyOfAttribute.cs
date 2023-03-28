using EasyDesk.Commons.Collections;
using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class RequireAnyOfAttribute : Attribute
{
    public RequireAnyOfAttribute(params object[] permissions)
    {
        Permissions = permissions.Select(x => new Permission(x.ToString())).ToEquatableSet();
    }

    public IImmutableSet<Permission> Permissions { get; }
}
