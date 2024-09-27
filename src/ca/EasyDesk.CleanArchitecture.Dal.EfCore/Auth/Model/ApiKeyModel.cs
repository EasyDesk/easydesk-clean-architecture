using EasyDesk.CleanArchitecture.Application.Authentication.ApiKey;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.Commons.Collections;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Auth.Model;

internal class ApiKeyModel
{
    public long Id { get; set; }

    public required string ApiKey { get; set; }

    public ICollection<ApiKeyIdentityModel> Identities { get; set; } = [];

    public Agent GetAgent() => Agent.FromIdentities(Identities.Select(x => x.ToIdentity()));

    public void UpdateIdentities(Agent agent)
    {
        Identities.Clear();

        agent.Identities
            .Select(x => ApiKeyIdentityModel.FromIdentity(x.Value))
            .AddTo(Identities);
    }

    public class Configuration : IEntityTypeConfiguration<ApiKeyModel>
    {
        public void Configure(EntityTypeBuilder<ApiKeyModel> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ApiKey)
                .HasMaxLength(ApiKeyValidator.MaxApiKeyLength);

            builder.HasIndex(x => x.ApiKey)
                .IsUnique();

            builder.OwnsMany(x => x.Identities, child =>
            {
                child
                    .WithOwner()
                    .HasForeignKey(x => x.ApiKeyId);

                child.HasKey(x => x.Id);

                child.OwnsMany(x => x.Attributes, grandchild =>
                {
                    grandchild.HasKey(x => x.Id);

                    grandchild
                        .WithOwner()
                        .HasForeignKey(x => x.IdentityId);
                });
            });
        }
    }
}
