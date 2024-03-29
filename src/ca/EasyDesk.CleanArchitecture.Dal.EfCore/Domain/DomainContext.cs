﻿using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using Microsoft.EntityFrameworkCore;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Domain;

public abstract class DomainContext : AbstractDbContext
{
    public const string SchemaName = "domain";

    protected DomainContext(DbContextOptions options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaName);

        ConfigureDomainModel(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    protected virtual void ConfigureDomainModel(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }
}
