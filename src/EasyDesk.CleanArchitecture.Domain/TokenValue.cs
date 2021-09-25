using EasyDesk.CleanArchitecture.Domain.Metamodel;
using System;

namespace EasyDesk.CleanArchitecture.Domain
{
    public record TokenValue : ValueWrapper<string>
    {
        private TokenValue(string value) : base(value)
        {
        }

        protected override void Validate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                // TODO: throw a more specific error.
                throw new ArgumentNullException(nameof(value));
            }
        }

        public override string ToString() => Value;

        public static TokenValue From(string value) => new(value);

        public static TokenValue Random() => From(Guid.NewGuid().ToString());
    }
}
