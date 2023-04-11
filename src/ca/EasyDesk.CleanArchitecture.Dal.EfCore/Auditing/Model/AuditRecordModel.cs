using EasyDesk.CleanArchitecture.Application.Auditing;
using EasyDesk.CleanArchitecture.Dal.EfCore.Interfaces.Abstractions;
using EasyDesk.CleanArchitecture.Dal.EfCore.Multitenancy;
using EasyDesk.Commons.Collections;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NodaTime;
using System.Linq.Expressions;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Auditing.Model;

internal class AuditRecordModel : IMultitenantEntity, IProjectable<AuditRecordModel, AuditRecord>
{
    public long Id { get; set; }

    required public AuditRecordType Type { get; set; }

    required public string Name { get; set; }

    required public string? Description { get; set; }

    required public string? UserId { get; set; }

    required public bool Success { get; set; }

    required public Instant Instant { get; set; }

    public string? TenantId { get; set; }

    public ICollection<AuditRecordPropertyModel> Properties { get; set; } = new HashSet<AuditRecordPropertyModel>();

    public class Configuration : IEntityTypeConfiguration<AuditRecordModel>
    {
        void IEntityTypeConfiguration<AuditRecordModel>.Configure(EntityTypeBuilder<AuditRecordModel> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

            builder.HasIndex(x => x.Instant).IsDescending();

            builder.OwnsMany(x => x.Properties, child =>
            {
                child.HasKey(x => new { x.AuditRecordId, x.Key });

                child.WithOwner().HasForeignKey(x => x.AuditRecordId);
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
            UserId = record.UserId.OrElseNull(),
            Success = record.Success,
            Instant = record.Instant,
        };

        model.Properties.AddAll(
            record.Properties.Select(p => new AuditRecordPropertyModel
            {
                Key = p.Key,
                Value = p.Value
            }));

        return model;
    }

    public static Expression<Func<AuditRecordModel, AuditRecord>> Projection() => src =>
        new(
            src.Type,
            src.Name,
            src.Description.AsOption(),
            src.UserId.AsOption(),
            src.Properties.Select(p => new KeyValuePair<string, string>(p.Key, p.Value)).ToEquatableMap(),
            src.Success,
            src.Instant);
}
