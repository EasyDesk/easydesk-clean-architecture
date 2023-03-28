using EasyDesk.CleanArchitecture.Domain.Metamodel.Values;

namespace EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;

public record AdminId : ValueWrapper<string, AdminId>
{
    private AdminId(string value) : base(value)
    {
    }

    public static AdminId From(string id) => new(id);
}
