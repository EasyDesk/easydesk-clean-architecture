using System;

namespace EasyDesk.CleanArchitecture.Domain.Metamodel
{
    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message)
        {
        }
    }
}
