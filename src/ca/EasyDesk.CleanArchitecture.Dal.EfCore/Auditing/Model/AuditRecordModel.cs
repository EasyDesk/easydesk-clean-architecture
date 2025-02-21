using EasyDesk.CleanArchitecture.Application.Auditing;
using EasyDesk.CleanArchitecture.Application.Authentication;
using EasyDesk.CleanArchitecture.Dal.EfCore.Multitenancy;
using EasyDesk.Commons.Collections;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NodaTime;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Auditing.Model;

internal class AuditRecordModel : IMultitenantEntity
{
    public long Id { get; set; }

    public required AuditRecordType Type { get; set; }

    public required string Name { get; set; }

    public required string? Description { get; set; }

    public required bool Success { get; set; }

    public required Instant Instant { get; set; }

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
                child.HasKey(x => new { x.AuditRecordId, x.Key, x.Value });

                child.WithOwner().HasForeignKey(x => x.AuditRecordId);
            });

            builder.OwnsMany(x => x.Identities, child =>
            {
                child.HasKey(x => new { x.AuditRecordId, x.IdentityRealm });

                child.Property(x => x.Identity)
                    .HasMaxLength(IdentityId.MaxLength);

                child.WithOwner().HasForeignKey(x => x.AuditRecordId);

                child.OwnsMany(x => x.IdentityAttributes, grandChild =>
                {
                    grandChild.HasKey(x => new { x.AuditRecordId, x.Realm, x.Key, x.Value });

                    grandChild.WithOwner().HasForeignKey(x => new { x.AuditRecordId, x.Realm });
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
            .SelectMany(p => p.Value.Select(x => new AuditRecordPropertyModel
            {
                Key = p.Key,
                Value = x,
            }))
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
                .Filter(identities => identities.HasAny())
                .Map(identities => Agent.FromIdentities(identities.Select(i => i.ToIdentity()))),
            Properties.GroupBy(p => p.Key, x => x.Value).ToFixedMap(x => x.Key, x => x.ToFixedSet()),
            Success,
            Instant);
    }
}
