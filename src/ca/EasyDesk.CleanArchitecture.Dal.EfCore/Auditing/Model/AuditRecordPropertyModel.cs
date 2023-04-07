using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Auditing.Model;

internal class AuditRecordPropertyModel
{
    public long AuditRecordId { get; set; }

    required public string Key { get; set; }

    required public string Value { get; set; }

    public class Configuration : IEntityTypeConfiguration<AuditRecordPropertyModel>
    {
        public void Configure(EntityTypeBuilder<AuditRecordPropertyModel> builder)
        {
            builder.HasKey(x => new { x.AuditRecordId, x.Key });
        }
    }
}
