﻿using EasyDesk.CleanArchitecture.Dal.EfCore.Repositories;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.SampleApp.Domain.Aggregates.PetAggregate;
using EasyDesk.SampleApp.Infrastructure.DataAccess.Model;
using Microsoft.EntityFrameworkCore;

namespace EasyDesk.SampleApp.Infrastructure.DataAccess.Repositories;

public class EfCorePetRepository : EfCoreRepositoryWithHydration<Pet, PetModel, SampleAppContext, int>, IPetRepository
{
    public EfCorePetRepository(SampleAppContext context, IDomainEventNotifier eventNotifier)
        : base(context, eventNotifier)
    {
    }

    public Task<bool> Exists(int id) => DbSet.AnyAsync(p => p.Id == id);

    public Task<Option<Pet>> GetById(int id) => GetSingle(q => q.Where(p => p.Id == id));

    public Task RemoveAll() => DbSet.ExecuteDeleteAsync();

    protected override IQueryable<PetModel> Includes(IQueryable<PetModel> initialQuery) => initialQuery;
}