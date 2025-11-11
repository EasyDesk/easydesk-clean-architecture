using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Application.Authorization.Static;
using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.Validation;
using EasyDesk.Commons.Results;
using EasyDesk.SampleApp.Application.Authorization;
using EasyDesk.SampleApp.Application.V_1_0.Dto;
using FluentValidation;

namespace EasyDesk.SampleApp.Application.V_1_0.Commands;

public record CreatePeople : ICommandRequest<IEnumerable<PersonDto>>, IAuthorize, IValidate<CreatePeople>
{
    public required IEnumerable<CreatePerson> People { get; init; }

    public static void ValidationRules(InlineValidator<CreatePeople> validator)
    {
        validator.RuleForEach(x => x.People).MustBeImplicitlyValid();
    }

    public bool IsAuthorized(AuthorizationInfo auth) => auth.HasPermission(Permissions.CanEditPeople);
}

public class CreatePeopleHandler : IHandler<CreatePeople, IEnumerable<PersonDto>>
{
    private readonly IDispatcher _dispatcher;

    public CreatePeopleHandler(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public async Task<Result<IEnumerable<PersonDto>>> Handle(CreatePeople request)
    {
        var results = new List<Result<PersonDto>>();
        foreach (var createPerson in request.People)
        {
            var result = await _dispatcher.Dispatch(createPerson);
            if (result.IsFailure && createPerson.FirstName == "skip")
            {
                continue;
            }
            if (result.IsFailure)
            {
                return result.ReadError();
            }
            results.Add(result);
        }
        return Success(results.Select(r => r.ReadValue()));
    }
}
