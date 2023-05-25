using EasyDesk.CleanArchitecture.Application.Validation;
using EasyDesk.Testing.MatrixExpansion;
using FluentValidation;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.Validation;

[UsesVerify]
public class PimpedAbstractValidatorTests
{
    public record TestRecord(Option<int> Value);

    private class TestValidator : PimpedAbstractValidator<TestRecord>
    {
        public TestValidator(Action<PimpedAbstractValidator<TestRecord>> validationRules)
        {
            validationRules(this);
        }
    }

    private void SimpleRules(PimpedAbstractValidator<TestRecord> validator)
    {
        validator.RuleForOption(x => x.Value)
            .GreaterThan(0);
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

    public static IEnumerable<object?[]> TestRecords()
    {
        return Matrix.Builder()
            .Axis<TestRecord>(new(None), new(Some(1)), new(Some(-1)))
            .Build();
    }
}
