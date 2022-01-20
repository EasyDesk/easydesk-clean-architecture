using EasyDesk.CleanArchitecture.Domain.Metamodel.Values;
using System;

namespace EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;

public record Role : ValueWrapper<string, Role>
{
    public const int MaxLength = 100;

    public Role(string value) : base(value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentNullException(nameof(value));
        }

        if (value.Length > MaxLength)
        {
            throw new ArgumentException($"Role must contain at most {MaxLength} characters", nameof(value));
        }
    }

    public static implicit operator Role(Enum value) => new(value.ToString());
}
