using EasyDesk.CleanArchitecture.Application.Mediator;
using EasyDesk.CleanArchitecture.Application.Pages;
using System;

namespace EasyDesk.SampleApp.Application.Queries
{
    public static class GetPeople
    {
        public record Query(Pagination Pagination) : PaginatedQueryBase<PersonSnapshot>(Pagination);

        public record PersonSnapshot(Guid Id, string Name, bool Married);
    }
}
