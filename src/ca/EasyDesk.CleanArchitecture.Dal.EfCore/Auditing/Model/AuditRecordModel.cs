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

    required public string? User { get; set; }

    required public bool Success { get; set; }

    required public Instant Instant { get; set; }

    public string? Tenant { get; set; }

    public ICollection<AuditRecordPropertyModel> Properties { get; set; } = new HashSet<AuditRecordPropertyModel>();

    public ICollection<AuditUserAttributeModel> UserAttributes { get; set; } = new HashSet<AuditUserAttributeModel>();

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

            builder.OwnsMany(x => x.UserAttributes, child =>
            {
                child.HasKey(x => x.Id);

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
            User = record.UserInfo.Map(u => u.UserId).MapToString().OrElseNull(),
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

        record.UserInfo.IfPresent(userInfo =>
        {
            userInfo
                .Attributes
                .Attributes
                .SelectMany(a => a.Value.Select(v => new AuditUserAttributeModel
                {
                    Key = a.Key,
                    Value = v,
                }))
                .AddTo(model.UserAttributes);
        });

        return model;
    }

    public AuditRecord ToAuditRecord()
    {
        return new(
            Type,
            Name,
            Description.AsOption(),
            User.AsOption().Map(id => UserInfo.Create(UserId.New(id), CreateAttributeCollection())),
            Properties.Select(p => new KeyValuePair<string, string>(p.Key, p.Value)).ToEquatableMap(),
            Success,
            Instant);
    }

    private AttributeCollection CreateAttributeCollection() =>
        AttributeCollection.FromFlatKeyValuePairs(UserAttributes.Select(a => (a.Key, a.Value)));
}
