using EasyDesk.CleanArchitecture.Dal.EfCore.ModelConversion;
using EasyDesk.CleanArchitecture.Domain.Model;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;
using EasyDesk.SampleApp.Infrastructure.DataAccess.Model;

namespace EasyDesk.SampleApp.Infrastructure.DataAccess.ModelConverters;

public class PersonConverter : IModelConverter<Person, PersonModel>
{
    public Person ToDomain(PersonModel model)
    {
        return new(model.Id, Name.From(model.FirstName), Name.From(model.LastName), model.DateOfBirth);
    }

    public void ApplyChanges(Person origin, PersonModel destination)
    {
        destination.Id = origin.Id;
        destination.FirstName = origin.FirstName;
        destination.LastName = origin.LastName;
        destination.DateOfBirth = origin.DateOfBirth;
    }
}
