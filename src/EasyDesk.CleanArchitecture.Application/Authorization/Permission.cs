using EasyDesk.CleanArchitecture.Domain.Metamodel.Values;
using System;

namespace EasyDesk.CleanArchitecture.Application.Authorization;

public record Permission : ValueWrapper<string, Permission>
{
    public const int MaxLength = 100;

    public Permission(string value) : base(value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentNullException(nameof(value));
        }

        if (value.Length > MaxLength)
        {
            throw new ArgumentException($"Permission must contain at most {MaxLength} characters", nameof(value));
        }
    }

    public static implicit operator Permission(Enum value) => new(value.ToString());
}
