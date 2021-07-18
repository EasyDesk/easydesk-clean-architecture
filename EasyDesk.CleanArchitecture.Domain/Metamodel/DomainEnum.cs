using System;

namespace EasyDesk.CleanArchitecture.Domain.Metamodel
{
    public class EnumValidationException<T> : ValidationException
        where T : struct
    {
        public EnumValidationException() : base($"Error parsing enum of type {typeof(T)}")
        {

        }
    }

    public static class DomainEnum
    {
        public static T Parse<T>(string enumValue)
            where T : struct
        {
            if (!Enum.TryParse<T>(enumValue, out var result))
            {
                throw new EnumValidationException<T>();
            }
            return result;
        }
    }
}
