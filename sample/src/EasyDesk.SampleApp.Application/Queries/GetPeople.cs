using EasyDesk.CleanArchitecture.Application.Mediator;
using EasyDesk.CleanArchitecture.Application.Pages;

namespace EasyDesk.SampleApp.Application.Queries;

public static class GetPeople
{
    public record Query(Pagination Pagination) : PaginatedQueryBase<PersonSnapshot>(Pagination);
}
