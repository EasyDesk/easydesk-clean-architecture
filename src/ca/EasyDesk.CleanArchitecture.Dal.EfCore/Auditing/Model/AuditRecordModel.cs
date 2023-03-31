using EasyDesk.CleanArchitecture.Application.Auditing;
using EasyDesk.CleanArchitecture.Dal.EfCore.Interfaces.Abstractions;
using EasyDesk.CleanArchitecture.Dal.EfCore.Multitenancy;
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

    public class Configuration : IEntityTypeConfiguration<AuditRecordModel>
    {
        void IEntityTypeConfiguration<AuditRecordModel>.Configure(EntityTypeBuilder<AuditRecordModel> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

            builder.HasIndex(x => x.Instant).IsDescending();
        }
    }

    public static AuditRecordModel Create(AuditRecord record) => new()
    {
        Type = record.Type,
        Name = record.Name,
        Description = record.Description.OrElseNull(),
        UserId = record.UserId.OrElseNull(),
        Success = record.Success,
        Instant = record.Instant,
    };

    public static Expression<Func<AuditRecordModel, AuditRecord>> Projection() => src =>
        new(src.Type, src.Name, src.Description.AsOption(), src.UserId.AsOption(), src.Success, src.Instant);
}
