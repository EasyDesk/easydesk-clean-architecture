using EasyDesk.CleanArchitecture.Application.Validation;
using EasyDesk.CleanArchitecture.Domain.Metamodel.Values.Validation;
using FluentValidation;

namespace EasyDesk.CleanArchitecture.UnitTests.Domain.Metamodel;

[UsesVerify]
public class CollectionValidationExtensionsTests
{
    public record Model(params string[] Values);

    [Fact]
    public async Task NotContainingDuplicates_ShouldReturnFailure_IfCollectionContainsDuplicates()
    {
        await Validate(new("A", "AA", "AA"), x => x.NotContainingDuplicates());
    }

    [Fact]
    public async Task NotContainingDuplicates_ShouldReturnSuccess_IfCollectionContainsNoDuplicates()
    {
        await Validate(new("A", "B", "C"), x => x.NotContainingDuplicates());
    }

    [Fact]
    public async Task NotContainingDuplicatesFor_ShouldReturnFailure_IfCollectionContainsDuplicates()
    {
        await Validate(new("A", "AA", "BB"), x => x.NotContainingDuplicatesFor(x => x.Length));
    }

    [Fact]
    public async Task NotContainingDuplicatesFor_ShouldReturnSuccess_IfCollectionContainsNoDuplicates()
    {
        await Validate(new("A", "BB", "CCC"), x => x.NotContainingDuplicatesFor(x => x.Length));
    }

    private static async Task Validate(Model model, Action<IRuleBuilder<Model, IEnumerable<string>>> configure)
    {
        var validator = new PimpedInlineValidator<Model>()
            .Also(x => configure(x.RuleFor(x => x.Values)));

        await Verify(validator.Validate(model));
    }
}
