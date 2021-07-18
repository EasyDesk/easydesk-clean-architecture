using EasyDesk.CleanArchitecture.Domain.Metamodel;
using System;
using System.Text.RegularExpressions;

namespace EasyDesk.CleanArchitecture.Domain.Model.Roles
{
    public record Role : ValueWrapper<string>
    {
        public const string Pattern = @"^[A-Za-z0-9_]+$";

        private Role(string value) : base(value)
        {
        }

        protected override void Validate(string value)
        {
            if (!Regex.IsMatch(value, Pattern))
            {
                throw new ArgumentException("The given string is not a valid role name", nameof(value));
            }
        }

        public override string ToString() => Value;

        public static Role From(string name) => new(name);
    }
}
