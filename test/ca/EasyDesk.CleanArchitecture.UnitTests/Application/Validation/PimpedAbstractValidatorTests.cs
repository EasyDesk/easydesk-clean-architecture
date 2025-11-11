using EasyDesk.CleanArchitecture.Application.Validation;
using EasyDesk.Commons.Options;
using FluentValidation;
using Shouldly;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.Validation;

public class PimpedAbstractValidatorTests
{
    public record TestRecord(Option<int> Number, Option<string> Text);

    public record NestedOption(Option<TestRecord> Child);

    private class TestValidator : AbstractValidator<TestRecord>
    {
        public TestValidator(Action<AbstractValidator<TestRecord>> validationRules)
        {
            validationRules(this);
        }
    }

    public static TheoryData<TestRecord> TestRecords()
    {
        var data = new TheoryData<TestRecord>();
        foreach (var number in new[] { None, Some(1), Some(-1), })
        {
            foreach (var text in new[] { None, Some(string.Empty), Some("aa"), Some("123456"), })
            {
                data.Add(new TestRecord(number, text));
            }
        }
        return data;
    }

    private void SimpleRules(AbstractValidator<TestRecord> validator)
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

    [Fact]
    public async Task ShouldValidate_NestedOption()
    {
        var validator = new InlineValidator<NestedOption>();
        validator.RuleForOption(x => x.Child, x => x.ChildRules(c => c.RuleForOption(x => x.Text, x => x.MinimumLength(5))));

        validator.Validate(new NestedOption(None)).Errors.ShouldBeEmpty();
        validator.Validate(new NestedOption(Some(new TestRecord(None, None)))).Errors.ShouldBeEmpty();
        validator.Validate(new NestedOption(Some(new TestRecord(None, Some("aaaaa"))))).Errors.ShouldBeEmpty();

        var result = validator.Validate(new NestedOption(Some(new TestRecord(None, Some("a")))));
        await Verify(result.Errors);
    }
}
