using Autofac;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.DependencyInjection;
using EasyDesk.CleanArchitecture.Domain.Metamodel.Repositories;
using EasyDesk.Commons.Tasks;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;
using EasyDesk.SampleApp.Infrastructure.EfCore;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using System.Data;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Transactions;

public class TransactionsTests : SampleIntegrationTest
{
    private static readonly Person _person = Person.Create(
            firstName: new("Pluto"),
            lastName: new("Paperino"),
            dateOfBirth: new(2020, 11, 11),
            createdBy: AdminId.From("admin"),
            residence: Address.Create(new("Street")));

    public TransactionsTests(SampleAppTestsFixture fixture) : base(fixture)
    {
    }

    protected override async Task OnInitialization()
    {
        await UseContext(async (context, scope) =>
        {
            var repo = scope.Resolve<IPersonRepository>();
            repo.Save(_person);
            await context.SaveChangesAsync();
        });
    }

    private async Task UseContext(AsyncAction<SampleAppContext, ILifetimeScope> action)
    {
        await using var scope = Session.Host.LifetimeScope.BeginUseCaseLifetimeScope();
        scope.Resolve<IContextTenantInitializer>().Initialize(TenantInfo.Public);
        await action(scope.Resolve<SampleAppContext>(), scope);
    }

    [Fact]
    public async Task ShouldSucceedForASingleTransaction()
    {
        await UseContext(async (context, scope) =>
        {
            await using var t = await context.Database.BeginTransactionAsync();

            var repo = scope.Resolve<IPersonRepository>();

            var p = await repo.FindById(_person.Id).Require();

            repo.Save(p);

            await context.SaveChangesAsync();

            await t.CommitAsync();
        });
    }

    [Fact]
    public async Task ShouldAbortSecondTransaction()
    {
        await UseContext(async (context1, scope1) =>
        {
            await UseContext(async (context2, scope2) =>
            {
                await using var t1 = await context1.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
                await using var t2 = await context2.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

                var repo1 = scope1.Resolve<IPersonRepository>();
                var repo2 = scope2.Resolve<IPersonRepository>();

                var p1 = await repo1.FindById(_person.Id).Require();
                var p2 = await repo2.FindById(_person.Id).Require();

                repo1.Save(p1);
                repo2.Save(p2);

                if (context1.ChangeTracker.HasChanges())
                {
                    await context1.SaveChangesAsync();
                }
                await t1.CommitAsync();

                var exception = await Should.ThrowAsync<Exception>(async () =>
                {
                    if (context2.ChangeTracker.HasChanges())
                    {
                        await context2.SaveChangesAsync();
                    }
                    await t2.CommitAsync();
                });
            });
        });
    }
}
