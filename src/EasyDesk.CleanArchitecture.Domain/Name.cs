﻿using EasyDesk.CleanArchitecture.Domain.Metamodel;
using System;

namespace EasyDesk.CleanArchitecture.Domain
{
    public record Name : ValueWrapper<string>
    {
        private Name(string name) : base(name)
        {
        }

        protected override void Validate(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                // TODO: throw a more specific error.
                throw new ArgumentException("A name must not be empty", nameof(name));
            }
        }

        public override string ToString() => Value;

        public static Name From(string name) => new(name);
    }
}