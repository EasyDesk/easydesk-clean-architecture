using EasyDesk.CleanArchitecture.Application.Auditing;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Dal.EfCore.Multitenancy;
using EasyDesk.Commons.Collections;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NodaTime;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Auditing.Model;

internal class AuditRecordModel : IMultitenantEntity
{
    public long Id { get; set; }

    required public AuditRecordType Type { get; set; }

    required public string Name { get; set; }

    required public string? Description { get; set; }

    required public bool Success { get; set; }

    required public Instant Instant { get; set; }

    public string? Tenant { get; set; }

    public ICollection<AuditRecordPropertyModel> Properties { get; set; } = new HashSet<AuditRecordPropertyModel>();

    public ICollection<AuditIdentityModel> Identities { get; set; } = new HashSet<AuditIdentityModel>();

    public sealed class Configuration : IEntityTypeConfiguration<AuditRecordModel>
    {
        public void Configure(EntityTypeBuilder<AuditRecordModel> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

            builder.HasIndex(x => x.Instant).IsDescending();

            builder.Property(x => x.Name).HasMaxLength(AuditModel.NameMaxLength);

            builder.OwnsMany(x => x.Properties, child =>
            {
                child.HasKey(x => new { x.AuditRecordId, x.Key });

                child.WithOwner().HasForeignKey(x => x.AuditRecordId);
            });

            builder.OwnsMany(x => x.Identities, child =>
            {
                child.HasKey(x => new { x.AuditRecordId, x.Name });

                child.Property(x => x.Identity)
                    .HasMaxLength(IdentityId.MaxLength);

                child.WithOwner().HasForeignKey(x => x.AuditRecordId);

                child.OwnsMany(x => x.IdentityAttributes, grandChild =>
                {
                    grandChild.HasKey(x => new { x.AuditRecordId, x.Name, x.Key, x.Value });

                    grandChild.WithOwner().HasForeignKey(x => new { x.AuditRecordId, x.Name });
                });
            });
        }
    }

    public static AuditRecordModel Create(AuditRecord record)
    {
        var model = new AuditRecordModel()
        {
            Type = record.Type,
            Name = record.Name,
            Description = record.Description.OrElseNull(),
            Success = record.Success,
            Instant = record.Instant,
        };

        record
            .Properties
            .Select(p => new AuditRecordPropertyModel
            {
                Key = p.Key,
                Value = p.Value
            })
            .AddTo(model.Properties);

        record.Agent.IfPresent(agent =>
        {
            agent
                .Identities
                .Select(kv => AuditIdentityModel.FromIdentity(kv.Key, kv.Value))
                .AddTo(model.Identities);
        });

        return model;
    }

    public AuditRecord ToAuditRecord()
    {
        return new(
            Type,
            Name,
            Description.AsOption(),
            Some(Identities)
                .Filter(identities => identities.Any())
                .Map(identities => Agent.FromIdentities(identities.Select(i => (i.Name, i.ToIdentity())))),
            Properties.Select(p => new KeyValuePair<string, string>(p.Key, p.Value)).ToEquatableMap(),
            Success,
            Instant);
    }
}
