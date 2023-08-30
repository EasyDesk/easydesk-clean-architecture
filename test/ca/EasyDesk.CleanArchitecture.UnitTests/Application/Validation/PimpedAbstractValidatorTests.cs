using EasyDesk.CleanArchitecture.Application.Validation;
using EasyDesk.Commons.Options;
using FluentValidation;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.Validation;

[UsesVerify]
public class PimpedAbstractValidatorTests
{
    public record TestRecord(Option<int> Number, Option<string> Text);

    private class TestValidator : PimpedAbstractValidator<TestRecord>
    {
        public TestValidator(Action<PimpedAbstractValidator<TestRecord>> validationRules)
        {
            validationRules(this);
        }
    }

    public static IEnumerable<object?[]> TestRecords()
    {
        foreach (var number in new[] { None, Some(1), Some(-1) })
        {
            foreach (var text in new[] { None, Some(string.Empty), Some("aa"), Some("123456") })
            {
                yield return new object[] { new TestRecord(number, text) };
            }
        }
    }

    private void SimpleRules(PimpedAbstractValidator<TestRecord> validator)
    {
        validator.RuleForOption(x => x.Number, rules => rules
            .GreaterThan(0));
        validator.RuleForOption(x => x.Text, rules => rules
            .NotEmpty()
            .MaximumLength(5)
            .WithErrorCode("tested-code")
            .WithMessage("tested-message")
            .OverridePropertyName("tested-property"));
    }

    [Theory]
    [MemberData(nameof(TestRecords))]
    public async Task ShouldValidate_UsingContext(
        TestRecord record)
    {
        var context = new ValidationContext<TestRecord>(record);
        var result = new TestValidator(SimpleRules).Validate(context);

        await Verify(result.Errors)
            .UseParameters(record);
    }

    [Theory]
    [MemberData(nameof(TestRecords))]
    public async Task ShouldValidate_UsingInstance(
        TestRecord record)
    {
        var result = new TestValidator(SimpleRules).Validate(record);
        await Verify(result.Errors)
            .UseParameters(record);
    }
}
