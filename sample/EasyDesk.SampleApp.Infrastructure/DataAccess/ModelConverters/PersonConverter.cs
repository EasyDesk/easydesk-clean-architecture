using EasyDesk.CleanArchitecture.Dal.EfCore.ModelConversion;
using EasyDesk.CleanArchitecture.Domain.Model;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;
using EasyDesk.SampleApp.Infrastructure.DataAccess.Model;

namespace EasyDesk.SampleApp.Infrastructure.DataAccess.ModelConverters;

public class PersonConverter : IModelConverter<Person, PersonModel>
{
    public Person ToDomain(PersonModel model)
    {
        return new(model.Id, Name.From(model.Name), model.Married);
    }

    public void ApplyChanges(Person origin, PersonModel destination)
    {
        destination.Id = origin.Id;
        destination.Name = origin.Name;
        destination.Married = origin.Married;
    }
}
