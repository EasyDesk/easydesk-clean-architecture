using EasyDesk.CleanArchitecture.Domain.Metamodel;
using System;
using System.Text.RegularExpressions;

namespace EasyDesk.CleanArchitecture.Domain.Roles
{
    public record RoleId : ValueWrapper<string>
    {
        public const string Pattern = @"^[A-Za-z0-9_]+$";

        private RoleId(string value) : base(value)
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

        public static RoleId From(string id) => new(id);
    }
}
