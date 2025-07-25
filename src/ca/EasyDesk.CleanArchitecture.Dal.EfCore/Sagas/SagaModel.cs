﻿using EasyDesk.CleanArchitecture.Dal.EfCore.Multitenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Sagas;

internal class SagaModel : IMultitenantEntity
{
    public required string Id { get; set; }

    public required string Type { get; set; }

    public string State { get; set; } = string.Empty;

    public byte[] State_Old { get; set; } = [];

    public int? Version { get; set; }

    public string? Tenant { get; set; }

    public sealed class Configuration : IEntityTypeConfiguration<SagaModel>
    {
        public void Configure(EntityTypeBuilder<SagaModel> builder)
        {
            builder.HasKey(x => new { x.Id, x.Type, x.Tenant, });

            builder.Property(x => x.Type).HasMaxLength(SagaManagerModel.SagaTypeMaxLength);
        }
    }
}
