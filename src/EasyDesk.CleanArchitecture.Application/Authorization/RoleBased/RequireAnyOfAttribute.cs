using EasyDesk.Tools.Collections;
using System;
using System.Collections.Immutable;
using System.Linq;

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
