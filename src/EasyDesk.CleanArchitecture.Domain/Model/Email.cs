using EasyDesk.CleanArchitecture.Domain.Metamodel.Values;
using System;
using System.Text.RegularExpressions;

namespace EasyDesk.CleanArchitecture.Domain.Model;

public record Email : ValueWrapper<string>
{
    public const string Pattern = @"^(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|""(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*"")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\[(?:(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9]))\.){3}(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9])|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])?";

    private Email(string email) : base(email)
    {
    }

    protected override void Validate(string email)
    {
        if (!Regex.IsMatch(email, Pattern))
        {
            // TODO: throw a more specific error.
            throw new ArgumentException("The given string is not a valid email address", nameof(email));
        }
    }

    public override string ToString() => Value;

    public static Email From(string email) => new(email);
}
