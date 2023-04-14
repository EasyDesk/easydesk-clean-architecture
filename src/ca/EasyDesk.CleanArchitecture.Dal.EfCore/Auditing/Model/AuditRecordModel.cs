using EasyDesk.CleanArchitecture.Application.Auditing;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
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

    required public string? User { get; set; }

    required public bool Success { get; set; }

    required public Instant Instant { get; set; }

    public string? Tenant { get; set; }

    public ICollection<AuditRecordPropertyModel> Properties { get; set; } = new HashSet<AuditRecordPropertyModel>();

    public sealed class Configuration : IEntityTypeConfiguration<AuditRecordModel>
    {
        public void Configure(EntityTypeBuilder<AuditRecordModel> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

            builder.HasIndex(x => x.Instant).IsDescending();

            builder.Property(x => x.Name).HasMaxLength(AuditModel.NameMaxLength);

            builder.Property(x => x.User).HasMaxLength(UserId.MaxLength);

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
            User = record.UserId.Map(u => u.Value).OrElseNull(),
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

    public static Expression<Func<AuditRecordModel, AuditRecord>> Projection()
    {
        var map = UserId.New;
        return src => new(
            src.Type,
            src.Name,
            src.Description.AsOption(),
            src.User.AsOption().Map(map),
            src.Properties.Select(p => new KeyValuePair<string, string>(p.Key, p.Value)).ToEquatableMap(),
            src.Success,
            src.Instant);
    }
}
