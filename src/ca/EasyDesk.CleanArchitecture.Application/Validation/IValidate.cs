using FluentValidation;

namespace EasyDesk.CleanArchitecture.Application.Validation;

public interface IValidate<T>
    where T : IValidate<T>
{
    private static IValidator<T> ConstructValidator()
    {
        var validator = new PimpedInlineValidator<T>();
        T.ValidationRules(validator);
        return validator;
    }

    public static IValidator<T> Validator { get; } = ConstructValidator();

    static abstract void ValidationRules(PimpedInlineValidator<T> validator);
}
