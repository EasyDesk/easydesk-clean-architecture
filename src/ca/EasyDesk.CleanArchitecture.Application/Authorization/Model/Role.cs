using EasyDesk.CleanArchitecture.Domain.Metamodel.Values;

namespace EasyDesk.CleanArchitecture.Application.Authorization.Model;

public record Role : ValueWrapper<string>
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
